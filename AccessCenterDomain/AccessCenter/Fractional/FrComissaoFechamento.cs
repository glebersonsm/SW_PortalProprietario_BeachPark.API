namespace AccessCenterDomain.AccessCenter.Fractional
{
    public class FrComissaoFechamento : EntityBase
    {
        public virtual int? ContaPagar { get; set; } // Vinculo com o Contas a Pagar;
        public virtual int? FrUsuario { get; set; } // Usuario/Pessoa vinculada ao lançamento de comissão;
        public virtual decimal? Total { get; set; } // Soma total de todos os lançamentos de comissão;
        public virtual int? MesReferencia { get; set; } // Mês Referencia para do fechamento;
        public virtual int? AnoReferencia { get; set; } // Ano Referencia para do fechamento;
        public virtual DateTime? DataInicial { get; set; } // Data de inicio de apuração das comisões;
        public virtual DateTime? DataFinal { get; set; } // Data final de apuração das comissões;
        public virtual int? Filial { get; set; }
        public virtual string? Observacao { get; set; } // Observação do fechamento;
        public virtual string? Liquidacao { get; set; } = "N";// Quanto for liquidação de comissão do web.
        public virtual string? DesligamentoColaborador { get; set; } = "N"; // Desligamento de alguem, caso sim, devo desconsiderar DataInicial e DataFinal;
        public virtual string? ConsiderarLancamentosAnt { get; set; } = "N"; // Caso sim, devo desconsiderar a dataInicial e buscar tudo do passado até a data final;

    }
}
