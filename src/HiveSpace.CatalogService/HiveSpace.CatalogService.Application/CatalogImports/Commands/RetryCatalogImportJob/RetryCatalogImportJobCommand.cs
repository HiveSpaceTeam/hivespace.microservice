using HiveSpace.Application.Shared.Commands;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;

namespace HiveSpace.CatalogService.Application.CatalogImports.Commands.RetryCatalogImportJob;

public record RetryCatalogImportJobCommand(Guid JobId) : ICommand<CatalogImportJobSubmissionDto>;
