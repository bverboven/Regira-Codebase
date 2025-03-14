﻿using Microsoft.Extensions.Logging;
using Regira.ProjectFilesProcessor.Models;
using Regira.System.Abstractions;
using Regira.TreeList;
using Regira.Utilities;

namespace Regira.ProjectFilesProcessor.Services;

public enum VersionType
{
    Build,
    Minor,
    Major
}
public class ProjectManager(
    ProjectService projectService,
    BaGetService baGetService,
    NuGetHelper nuGetHelper,
    IProcessHelper processHelper,
    ILogger<ProjectManager> logger)
{
    public IList<Project>? Projects { get; private set; }
    /// <summary>
    /// A tree-list where a parent-Project is a dependency for other project(s)
    /// </summary>
    public ProjectTree? ProjectTree { get; private set; }

    public void PushPending()
    {
        var pendingProjects = GetPendingProjects();
        foreach (var project in pendingProjects)
        {
            logger.LogInformation($"Pushing {project.Id} (v{project.PublishedVersion} -> v{project.Version})");
            var cmd = nuGetHelper.CreateNuGetCommand(project);
            var response = processHelper.ExecuteCommand(cmd, true);
            if (response.ExitCode == 0)
            {
                var output = response.Output?.Trim().Split(Environment.NewLine).LastOrDefault()?.Trim();
                var isSkipped = output?.StartsWith("--skip-duplicate") == true;
                logger.LogInformation(isSkipped ? "(skipped: duplicate)" : $"{output}");
            }
            else
            {
                logger.LogWarning($"Pushing {project.Id} failed:{Environment.NewLine}{response.Output}");
            }
        }
    }

    // legacy
    public IList<string> GetBatchCommand()
    {
        return Projects!.Select(nuGetHelper.CreateNuGetCommand).ToArray();
    }

    public async Task Init()
    {
        var projects = (await projectService.List())
            .Where(x => x.IsClassLibrary == true)
            .ToArray();

        var publishedProjects = (await baGetService.List()).AsList();
        SetPublishedVersions(projects, publishedProjects);

        // Projects
        Projects = projects;

        // Project Tree
        ProjectTree = ProjectTree.Load(Projects);
    }
    public void CheckAndUpdateVersions()
    {
        foreach (var projectNode in ProjectTree!)
        {
            CheckVersionWithOffspring(projectNode);
        }
    }
    public async Task SaveProjectFiles()
    {
        foreach (var project in Projects!)
        {
            await projectService.Save(project);
            logger.LogInformation($"Saved {project.Id ?? project.ProjectFile} (v{project.Version})");
        }
    }
    public IList<Project> GetPendingProjects()
    {
        return Projects!
            .Where(x => x.GeneratePackageOnBuild && !string.IsNullOrWhiteSpace(x.Id))
            .Where(x => x.PublishedVersion == null || x.Version > x.PublishedVersion)
            .ToArray();
    }
    void SetPublishedVersions(IList<Project> projects, IList<Project> publishedProjects)
    {
        foreach (var project in projects)
        {
            var publishedProject = publishedProjects.FirstOrDefault(x => x.Id == project.Id);
            if (publishedProject == null)
            {

            }
            // set published version
            project.PublishedVersion = publishedProject?.Version;
        }
    }
    void CheckVersionWithOffspring(TreeNode<Project> projectNode, VersionType increaseType = VersionType.Build)
    {
        var project = projectNode.Value;

        if (project.PublishedVersion == null || project.Version > project.PublishedVersion)
        {
            if (project.Version.Major > project.PublishedVersion?.Major)
            {
                increaseType = VersionType.Major;
            }
            else if (project.Version.Minor > project.PublishedVersion?.Minor)
            {
                increaseType = VersionType.Minor;
            }
            foreach (var childNode in projectNode.Children)
            {
                var childProject = childNode.Value;
                var childVersion = childProject.Version;
                if (childProject.Version <= childProject.PublishedVersion)
                {
                    // increase version-build
                    switch (increaseType)
                    {
                        case VersionType.Major:
                            childProject.Version = new Version($"{childVersion.Major + 1}.0.0");
                            break;
                        case VersionType.Minor:
                            childProject.Version = new Version($"{childVersion.Major}.{childVersion.Minor + 1}.0");
                            break;
                        default: // VersionType.Build
                            childProject.Version = new Version($"{childVersion.Major}.{childVersion.Minor}.{childVersion.Build + 1}");
                            break;
                    }
                }
                CheckVersionWithOffspring(childNode, increaseType);
            }
        }
    }
}