using System.Text.Json;
using FunktsiooniUurimine.Data;
using FunktsiooniUurimine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FunctionModel = FunktsiooniUurimine.Models.FunktsiooniUurimine;

namespace FunktsiooniUurimine.Controllers;

public class FunktsiooniUurimisedController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly MatemaatikaService _matemaatikaService;

    public FunktsiooniUurimisedController(
        ApplicationDbContext context, MatemaatikaService matemaatikaService)
    {
        _context = context;
        _matemaatikaService = matemaatikaService;
    }

    public async Task<IActionResult> Index() =>
        View(await _context.FunktsiooniUurimised.AsNoTracking().ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var uurimine = await _context.FunktsiooniUurimised
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (uurimine == null)
        {
            return NotFound();
        }

        const double algus = -5;
        const double lopp = 5;
        const double samm = 0.1;
        ViewBag.XVaartused = JsonSerializer.Serialize(
            Enumerable.Range(0, 101).Select(i => algus + i * samm));
        ViewBag.FunktsiooniPunktid = JsonSerializer.Serialize(
            _matemaatikaService.ArvutaGraafikuPunktid(uurimine.Valem, algus, lopp, samm));
        ViewBag.TuletisePunktid = JsonSerializer.Serialize(
            _matemaatikaService.ArvutaGraafikuPunktid(uurimine.Valem, algus, lopp, samm, true));

        return View(uurimine);
    }

    public IActionResult Create() => View(new FunctionModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Valem")] FunctionModel uurimine)
    {
        if (!ModelState.IsValid || !_matemaatikaService.TryAnalyze(uurimine))
        {
            LisaValemiViga();
            return View(uurimine);
        }

        uurimine.LuodudAeg = DateTime.UtcNow;
        await _context.FunktsiooniUurimised.AddAsync(uurimine);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var uurimine = await _context.FunktsiooniUurimised.FindAsync(id);
        return uurimine == null ? NotFound() : View(uurimine);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Valem")] FunctionModel vorm)
    {
        if (id != vorm.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(vorm);
        }

        var uurimine = await _context.FunktsiooniUurimised.FindAsync(id);
        if (uurimine == null)
        {
            return NotFound();
        }

        uurimine.Valem = vorm.Valem;
        if (!_matemaatikaService.TryAnalyze(uurimine))
        {
            ModelState.AddModelError(nameof(vorm.Valem), "Kontrolli valemit. Kasuta muutujat x ja toetatud matemaatilisi tehteid.");
            return View(vorm);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var uurimine = await _context.FunktsiooniUurimised
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);
        return uurimine == null ? NotFound() : View(uurimine);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var uurimine = await _context.FunktsiooniUurimised.FindAsync(id);
        if (uurimine != null)
        {
            _context.FunktsiooniUurimised.Remove(uurimine);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private void LisaValemiViga()
    {
        if (ModelState.IsValid)
        {
            ModelState.AddModelError(nameof(FunctionModel.Valem), "Kontrolli valemit. Kasuta muutujat x ja toetatud matemaatilisi tehteid.");
        }
    }
}