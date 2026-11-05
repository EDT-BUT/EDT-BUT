using Microsoft.AspNetCore.Mvc;

namespace Edt.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class CustomControllerBase : ControllerBase;
