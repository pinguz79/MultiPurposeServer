using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Services;

namespace Portfolio.Api.Controllers;

public abstract class PortfolioControllerBase(IAlbumService albumService) : ControllerBase
{
    protected IAlbumService AlbumService { get; } = albumService;
}
