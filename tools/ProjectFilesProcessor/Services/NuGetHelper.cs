﻿using Regira.ProjectFilesProcessor.Models;

namespace Regira.ProjectFilesProcessor.Services;

public class NuGetHelper
{

    public class Options
    {
        public string? ApiKey { get; set; }
        public string PackagesPushUri { get; set; } = null!;
    }


    private readonly Options _options;
    public NuGetHelper(Options options)
    {
        _options = options;
    }


    /// <summary>
    /// Creates a build- and a push-command
    /// </summary>
    /// <param name="project"></param>
    /// <returns>Command</returns>
    /// <exception cref="ArgumentException">When <see cref="Project.ProjectFile"/> is missing</exception>
    public string CreateNuGetCommand(Project project)
        => $"{CreateBuildCommand(project)}{Environment.NewLine}{CreatePushCommand(project)}{Environment.NewLine}";


    public string CreateBuildCommand(Project project)
    {
        if (string.IsNullOrWhiteSpace(project.ProjectFile))
        {
            throw new ArgumentException($"Missing {nameof(Project.ProjectFile)} in project {project.Id}");
        }

        return $@"dotnet build ""{project.ProjectFile}"" --configuration Release";
    }
    public string CreatePushCommand(Project project)
    {
        if (string.IsNullOrWhiteSpace(project.ProjectFile))
        {
            throw new ArgumentException($"Missing {nameof(Project.ProjectFile)} in project {project.Id}");
        }

        var projectDirectory = Path.GetDirectoryName(project.ProjectFile);
        if (!Directory.Exists(projectDirectory))
        {
            throw new DirectoryNotFoundException($"Directory {projectDirectory} not found for project {project.Id}");
        }

        return $@"dotnet nuget push -s ""{_options.PackagesPushUri}""{(!string.IsNullOrWhiteSpace(_options.ApiKey) ? $@" -k ""{_options.ApiKey}""" : "")} ""{Path.Combine(projectDirectory, @"bin\Release", $"{project.Id}.{project.Version}.nupkg")}""";
    }
}