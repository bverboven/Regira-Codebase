﻿namespace Regira.IO.Storage.SSH;

public class SftpConfig
{
    public string? Host { get; set; }
    public int Port { get; set; } = 22;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? ContainerName { get; set; } = "/";
}