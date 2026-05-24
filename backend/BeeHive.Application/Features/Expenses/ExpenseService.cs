using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Expenses.DTOs;
using BeeHive.Domain.Entities;

namespace BeeHive.Application.Features.Expenses;

// ── Interface ────────────────────────────────────────────────────────────────

public interface IExpenseService
{
    Task<IEnumerable<ExpenseDto>> GetByOrganizationAsync(int orgId);
    Task<ExpenseDetailDto> GetByIdAsync(int id, int orgId);
    Task<ExpenseDetailDto> CreateAsync(CreateExpenseDto dto, int orgId, int? createdById);
    Task<ExpenseDetailDto> UpdateAsync(int id, UpdateExpenseDto dto, int orgId);
    Task DeleteAsync(int id, int orgId);
}

// ── Implementation ───────────────────────────────────────────────────────────

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ExpenseService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ExpenseDto>> GetByOrganizationAsync(int orgId)
    {
        var expenses = await _uow.Expenses.GetByOrganizationAsync(orgId);
        return _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
    }

    public async Task<ExpenseDetailDto> GetByIdAsync(int id, int orgId)
    {
        var expense = await _uow.Expenses.GetWithItemsAsync(id)
            ?? throw new NotFoundException(nameof(Expense), id);

        if (expense.OrganizationId != orgId)
            throw new NotFoundException(nameof(Expense), id);

        return _mapper.Map<ExpenseDetailDto>(expense);
    }

    public async Task<ExpenseDetailDto> CreateAsync(CreateExpenseDto dto, int orgId, int? createdById)
    {
        var expense = _mapper.Map<Expense>(dto);
        expense.OrganizationId = orgId;
        expense.CreatedById = createdById;

        for (int i = 0; i < expense.Items.Count; i++)
            expense.Items[i].SortOrder = i;

        await _uow.Expenses.AddAsync(expense);
        await _uow.SaveChangesAsync();

        var created = await _uow.Expenses.GetWithItemsAsync(expense.Id)
            ?? throw new InvalidOperationException("Expense was not saved correctly.");

        return _mapper.Map<ExpenseDetailDto>(created);
    }

    public async Task<ExpenseDetailDto> UpdateAsync(int id, UpdateExpenseDto dto, int orgId)
    {
        var expense = await _uow.Expenses.GetWithItemsAsync(id)
            ?? throw new NotFoundException(nameof(Expense), id);

        if (expense.OrganizationId != orgId)
            throw new NotFoundException(nameof(Expense), id);

        // Replace item collection — remove old, add new
        expense.Items.Clear();
        _mapper.Map(dto, expense);

        for (int i = 0; i < expense.Items.Count; i++)
            expense.Items[i].SortOrder = i;

        expense.UpdatedAt = DateTime.UtcNow;

        await _uow.Expenses.UpdateAsync(expense);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Expenses.GetWithItemsAsync(id)!;
        return _mapper.Map<ExpenseDetailDto>(updated!);
    }

    public async Task DeleteAsync(int id, int orgId)
    {
        var expense = await _uow.Expenses.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Expense), id);

        if (expense.OrganizationId != orgId)
            throw new NotFoundException(nameof(Expense), id);

        await _uow.Expenses.DeleteAsync(expense);
        await _uow.SaveChangesAsync();
    }
}
