using Regira.Media.Drawing.Models.DTO;

namespace Regira.Office.Clients.Models;

public class AggregateInputDto : DrawImageLayerDto
{
    public ImageLayerOptionsDto? DrawOptions { get; set; }
}