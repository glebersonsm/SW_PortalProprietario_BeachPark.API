using System.ComponentModel.DataAnnotations;

namespace CMDomain.Entities
{
    public class ReqMat : CMEntityBase
    {
        [Key, Required]
        public virtual long NumRequisicao { get; set; }
        public virtual int? UnidNegoc { get; set; }
        public virtual int? IdProcesso { get; set; }
        public virtual int? IdUsuarioInclusao { get; set; }
        public virtual string? CustoTransf { get; set; } = "T";
        //D = Devolucao de custo
        //C = Custo
        //T = Transferência
        //E = Outra empresa
        //F = Transferência e custo

        public virtual string? CodCentroCusto { get; set; }
        public virtual DateTime? DataEmissao { get; set; }
        public virtual string? ReqAtendida { get; set; } = "F";
        public virtual int? IdEmpresa { get; set; }//Se relaciona com IdPessoa Origem e CodAlmoxaOrigem
        public virtual int? CodAlmoxaOrigem { get; set; }//Se relaciona com IdEmpresa e IdPessoaOrigem
        public virtual int? IdPessoaOrigem { get; set; }//Se relaciona com IdEmpresa e CodAlmoxaOrigem

        public virtual int? IdPessoa { get; set; } //Se relaciona com IdEmpresaDestino
        public virtual int? CodAlmoxaDestino { get; set; }//Se relaciona com IdPessoa e IdEmpresaDestino
        public virtual int? IdEmpresaDestino { get; set; }//Se relaciona com IdPessoa e CodAlmoxaDestino

        public virtual DateTime? DataNecessidade { get; set; }
        public virtual string? Impresso { get; set; } = "F";
        public virtual string? Obs { get; set; }
        public virtual int? IdEvento { get; set; }
        public virtual int? IdNotaTransf { get; set; }
        public virtual string? CodCentroCustoOrigem { get; set; }
        public virtual int? UnidNegocOrigem { get; set; }

        public virtual DateTime? TrgDtInclusao { get; set; }
        public virtual string? TrgUserInclusao { get; set; }
    }
}
