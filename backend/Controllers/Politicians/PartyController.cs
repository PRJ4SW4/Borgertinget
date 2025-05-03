namespace backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using backend.Services;
using backend.Models; 
using backend.Data;
using backend.DTO.FT;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

[ApiController]
[Route("api/[controller]")]
public class PartyController : ControllerBase{
    private readonly DataContext _context;

    public PartyController(DataContext context){
        _context = context;
    }



    [HttpGet("Parties")]
    public async Task<ActionResult<IEnumerable<Party>>> getParties(){
        var parties = await _context.Party.
                                    OrderBy(p => p.partyName).
                                    ToListAsync();
        return Ok(parties);
    }

    [HttpGet("Party/{partyName}")]
    public async Task<ActionResult<Party>> Party(string partyName){
        var party = await _context.Party.FindAsync(partyName);
        if (party == null){
            return NotFound($"No party found with {partyName}");
        }
        return Ok(party);

    }
}