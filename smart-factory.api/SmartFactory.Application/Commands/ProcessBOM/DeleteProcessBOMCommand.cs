using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Data;

namespace SmartFactory.Application.Commands.ProcessBOM;

/// <summary>
/// Command to delete a Process BOM (only if not ACTIVE)
/// </summary>
public class DeleteProcessBOMCommand : IRequest<DeleteProcessBOMResult>
{
    public Guid Id { get; set; }
}

public class DeleteProcessBOMResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class DeleteProcessBOMCommandHandler : IRequestHandler<DeleteProcessBOMCommand, DeleteProcessBOMResult>
{
    private readonly ApplicationDbContext _context;

    public DeleteProcessBOMCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteProcessBOMResult> Handle(DeleteProcessBOMCommand request, CancellationToken cancellationToken)
    {
        var bom = await _context.ProcessBOMs
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (bom == null)
        {
            return new DeleteProcessBOMResult
            {
                Success = false,
                Message = "Process BOM không tồn tại"
            };
        }

        // Không cho phép xóa BOM đang ACTIVE
        if (bom.Status == "ACTIVE")
        {
            return new DeleteProcessBOMResult
            {
                Success = false,
                Message = "Không thể xóa BOM đang ở trạng thái ACTIVE. Vui lòng tạo BOM mới để thay thế."
            };
        }

        // Xóa BOM details trước
        var bomDetails = await _context.ProcessBOMDetails
            .Where(d => d.ProcessBOMId == request.Id)
            .ToListAsync(cancellationToken);

        _context.ProcessBOMDetails.RemoveRange(bomDetails);
        
        // Xóa BOM
        _context.ProcessBOMs.Remove(bom);
        
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteProcessBOMResult
        {
            Success = true,
            Message = "Đã xóa Process BOM thành công"
        };
    }
}

