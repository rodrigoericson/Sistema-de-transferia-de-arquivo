using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using STA.Api.Common;

namespace STA.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/diretorios")]
public class DiretoriosController : ControllerBase
{
    [HttpPost("validar")]
    public ActionResult<ApiResponse<ValidacaoDiretorioResult>> Validar([FromBody] ValidarDiretorioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
            return BadRequest(new ApiResponse<ValidacaoDiretorioResult>(false, null, "Caminho é obrigatório."));

        var path = request.Path.Trim();

        // Verifica se já existe
        if (Directory.Exists(path))
        {
            return Ok(new ApiResponse<ValidacaoDiretorioResult>(true, new ValidacaoDiretorioResult("existe", "Diretório encontrado.", true)));
        }

        // Tenta criar
        try
        {
            Directory.CreateDirectory(path);
            return Ok(new ApiResponse<ValidacaoDiretorioResult>(true, new ValidacaoDiretorioResult("criado", "Diretório criado com sucesso.", true)));
        }
        catch (UnauthorizedAccessException)
        {
            return Ok(new ApiResponse<ValidacaoDiretorioResult>(true, new ValidacaoDiretorioResult("sem_permissao", "Sem permissão para criar o diretório.", false)));
        }
        catch (IOException ex)
        {
            return Ok(new ApiResponse<ValidacaoDiretorioResult>(true, new ValidacaoDiretorioResult("inacessivel", $"Caminho inacessível: {ex.Message}", false)));
        }
        catch (Exception ex)
        {
            return Ok(new ApiResponse<ValidacaoDiretorioResult>(true, new ValidacaoDiretorioResult("erro", $"Erro: {ex.Message}", false)));
        }
    }
}

public record ValidarDiretorioRequest(string Path);
public record ValidacaoDiretorioResult(string Status, string Mensagem, bool Ok);
