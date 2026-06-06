using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<Expense>> GetByOrganizationAsync(int organizationId) =>
        await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Items)
            .Include(e => e.CreatedBy)
            .Where(e => e.OrganizationId == organizationId)
            .OrderByDescending(e => e.PurchaseDate)
            .ThenByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<Expense?> GetWithItemsAsync(int id) =>
        await _context.Expenses
            .Include(e => e.Items.OrderBy(i => i.SortOrder))
            .Include(e => e.CreatedBy)
            .FirstOrDefaultAsync(e => e.Id == id);
}
