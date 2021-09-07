﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioNotasConceitos : IRepositorioBase<NotaConceito>
    {
        IEnumerable<NotaConceito> ObterNotasPorAlunosAtividadesAvaliativas(IEnumerable<long> atividadesAvaliativas,
            IEnumerable<string> alunosIds, string disciplinaId);

        Task<IEnumerable<NotaConceito>> ObterNotasPorAlunosAtividadesAvaliativasAsync(long[] atividadesAvaliativasId,
            string[] alunosIds, string componenteCurricularId);

        Task<NotaConceito> ObterNotasPorGoogleClassroomIdTurmaIdComponentCurricularId(long atividadeClassroomId, string turmaId, string componenteCurricularId);
        Task<NotaConceito> ObterNotasPorId(long id);
    }
}