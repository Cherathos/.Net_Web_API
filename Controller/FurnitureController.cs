using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class FurnitureController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FurnitureController> _logger;

    public FurnitureController(ApplicationDbContext context,
    ILogger<FurnitureController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Route("GetAllFurnitures")]
    [Authorize(Roles = "User,Admin")]
    public IActionResult GetAllFurnitures()
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);
        return Ok(_context.Furnitures.ToList());
    }

    [HttpGet]
    [Route("GetFurnitureById/{id:guid}")]
    [Authorize(Roles = "User,Admin")]
    public IActionResult GetFurnitureById(Guid id)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);
        var furniture = _context.Furnitures.Find(id);
        if (furniture is null)
        {
            return NotFound();
        }
        return Ok(furniture);
    }

    [HttpPost]
    [Route("AddFurniture")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddFurniture(addFurnitureDTO addFurnitureDTO)
    {
        if(!ModelState.IsValid){
            _logger.LogInformation("Invalid model state for AddFurniture request");
            return BadRequest(ModelState);
        }
        var furniture = new Furniture()
        {
            Name = addFurnitureDTO.Name,
            Price = addFurnitureDTO.Price,
            Description = addFurnitureDTO.Description
        };
        _context.Furnitures.Add(furniture);
        await _context.SaveChangesAsync();
        _logger.LogInformation("New furniture added: {FurnitureName}", furniture.Name);
        return Ok(furniture);
    }

    [HttpDelete]
    [Route("DeleteFurniture/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFurniture(Guid id)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);
        var existingFurniture = await _context.Furnitures.FindAsync(id);
        if (existingFurniture is null)
        {
            return NotFound();
        }

        _context.Furnitures.Remove(existingFurniture);
        await _context.SaveChangesAsync();
        _logger.LogWarning("Furniture deleted: {FurnitureName}", existingFurniture.Name);
        return Ok(existingFurniture);
    }

    [HttpPut]
    [Route("EditFurniture/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditFurniture(Guid id, addFurnitureDTO furniture)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);
        var existingFurniture = await _context.Furnitures.FindAsync(id);
        if (existingFurniture is null)
        {
            return NotFound();
        }

        existingFurniture.Name = furniture.Name;
        existingFurniture.Price = furniture.Price;
        existingFurniture.Description = furniture.Description;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Furniture edited: {FurnitureName}", existingFurniture.Name);
        return Ok(existingFurniture);
    }
}