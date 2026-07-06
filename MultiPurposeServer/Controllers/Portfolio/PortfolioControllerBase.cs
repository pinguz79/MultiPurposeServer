using Microsoft.AspNetCore.Mvc;
using MultiPurposeServer.Microservices.Portfolio;

namespace MultiPurposeServer.Controllers.Portfolio
{
    public abstract class PortfolioControllerBase(IAlbumService albumService) : ControllerBase
    {
    }
}
