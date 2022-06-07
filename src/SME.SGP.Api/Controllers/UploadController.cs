﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SME.SGP.Aplicacao;
using SME.SGP.Infra;
using System.Linq;
using System.Threading.Tasks;

namespace SME.SGP.Api.Controllers
{
    /// <summary>
    /// Controller para upload de arquivos do Jodit
    /// </summary>
    [ApiController]
    [Route("api/v1/arquivos/upload")]
    [Authorize("Bearer")]
    public class UploadController : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(RetornoBaseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(RetornoBaseDto), 500)]
        public async Task<IActionResult> Editor([FromServices] IUploadArquivoEditorUseCase useCase)
        {
            var files = Request.Form.Files;
            if (files != null)
            {
                var file = files.FirstOrDefault();
                string urlBase = Request.Host.Value;
                if (file.Length > 0)
                    return Ok(await useCase.Executar(files.FirstOrDefault(), 
                        $"https://{urlBase}{Request.PathBase}{ArquivoContants.PastaAquivosTemporaria}", 
                        Dominio.TipoArquivo.Editor));
            }
            return BadRequest();
        }
    }
}
