USE [master]
GO
/****** Object:  Database [se1426]    Script Date: 03/05/2021 10:48:54 ******/
CREATE DATABASE [se1426]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'se1426', FILENAME = N'D:\Dados\EOL\se1426_base_eol' , SIZE = 204800000KB , MAXSIZE = UNLIMITED, FILEGROWTH = 10%)
 LOG ON 
( NAME = N'se1426_log', FILENAME = N'E:\Log\EOL\se1426_base_eol_log' , SIZE = 203797760KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [se1426] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [se1426].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [se1426] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [se1426] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [se1426] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [se1426] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [se1426] SET ARITHABORT OFF 
GO
ALTER DATABASE [se1426] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [se1426] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [se1426] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [se1426] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [se1426] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [se1426] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [se1426] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [se1426] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [se1426] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [se1426] SET  DISABLE_BROKER 
GO
ALTER DATABASE [se1426] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [se1426] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [se1426] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [se1426] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [se1426] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [se1426] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [se1426] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [se1426] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [se1426] SET  MULTI_USER 
GO
ALTER DATABASE [se1426] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [se1426] SET DB_CHAINING OFF 
GO
ALTER DATABASE [se1426] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [se1426] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [se1426] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'se1426', N'ON'
GO
USE [se1426]
GO
/****** Object:  User [user_se1426_cotic]    Script Date: 03/05/2021 10:48:56 ******/
CREATE USER [user_se1426_cotic] FOR LOGIN [user_se1426_cotic] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [EDUCACAO\sv-bkpagent]    Script Date: 03/05/2021 10:48:56 ******/
CREATE USER [EDUCACAO\sv-bkpagent] FOR LOGIN [EDUCACAO\sv-bkpagent] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [user_se1426_cotic]
GO
ALTER ROLE [db_backupoperator] ADD MEMBER [EDUCACAO\sv-bkpagent]
GO
/****** Object:  UserDefinedFunction [dbo].[proc_existe_historico_matricula_diferente_aluno]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[proc_existe_historico_matricula_diferente_aluno]
(
	@p_cd_aluno int,
	@p_an_letivo smallint,
	@p_cd_matricula_comparacao int,
	@p_cd_serie_ensino int
)
RETURNS BIT
AS
BEGIN
	DECLARE @existe_matricula BIT = ISNULL(
	(
		SELECT TOP 1 1
			FROM v_historico_matricula_cotic
		WHERE an_letivo = @p_an_letivo AND
			  cd_aluno = @p_cd_aluno AND
			  cd_matricula <> @p_cd_matricula_comparacao AND
			  cd_serie_ensino = @p_cd_serie_ensino			  
		ORDER BY dt_status_matricula DESC
	), 0);
	
	RETURN @existe_matricula
END
GO
/****** Object:  UserDefinedFunction [dbo].[proc_gerar_email_alternativo_aluno]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 
-- Function para geracao do email
--
CREATE FUNCTION [dbo].[proc_gerar_email_alternativo_aluno](@p_nm_aluno as VARCHAR(128), @p_dt_nascimento_aluno AS DATETIME, @p_separador_nome AS CHAR)
RETURNS VARCHAR(256)
BEGIN
    DECLARE @palavras_nomes TABLE( id INT, elemento VARCHAR(64) );
    DECLARE @ultimo_indice_palavras AS INT;
    DECLARE @str_dt_nascimento AS VARCHAR(16);
    DECLARE @nm_email AS VARCHAR(128);
    
    DECLARE @dominio_email AS VARCHAR(32);
    SET @dominio_email = '@edu.sme.prefeitura.sp.gov.br';

    -- Extrai as palavras no nome do aluno e a quantidade
    INSERt into @palavras_nomes 
        SELECT ROW_NUMBER() OVER (ORDER BY (select null)) as id, elemento 
        FROM proc_string_split(@p_nm_aluno, ' ');
        
    SET @ultimo_indice_palavras = (SELECT COUNT(id) FROM @palavras_nomes);

    SET @p_nm_aluno = '';
    SELECT @p_nm_aluno = COALESCE(@p_nm_aluno + (
        CASE
            WHEN id = 1 THEN elemento
			WHEN id = @ultimo_indice_palavras THEN @p_separador_nome + elemento
            ELSE CASE 
                WHEN LOWER(elemento) IN ('da', 'das', 'de', 'do', 'dos', 'os') THEN ''
                ELSE LEFT(elemento, 1)
            END 
        END
    ), elemento) FROM @palavras_nomes;
        
    -- Extrai e formata a data de nascimento
    SET @str_dt_nascimento = (
        RIGHT('00' + LTRIM(CAST(DATEPART(DAY, @p_dt_nascimento_aluno) as NVARCHAR(2))), 2) + 
        RIGHT('00' + LTRIM(CAST(DATEPART(MONTH, @p_dt_nascimento_aluno) as NVARCHAR(2))), 2) + 
        RIGHT('0000' + LTRIM(CAST(DATEPART(YEAR, @p_dt_nascimento_aluno) as NVARCHAR(4))), 4)
    );

    -- Monta o email
    SET @nm_email = LOWER(@p_nm_aluno + '.' + @str_dt_nascimento + @dominio_email);
    RETURN @nm_email;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[proc_gerar_email_aluno]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- 
-- Function para geracao do email
--
create FUNCTION [dbo].[proc_gerar_email_aluno](@p_nm_aluno as VARCHAR(128), @p_dt_nascimento_aluno AS DATETIME)
RETURNS VARCHAR(256)
BEGIN
    DECLARE @palavras_nomes TABLE( id INT, elemento VARCHAR(64) );
    DECLARE @ultimo_indice_palavras AS INT;
    DECLARE @str_dt_nascimento AS VARCHAR(16);
    DECLARE @nm_email AS VARCHAR(128);
    
    DECLARE @dominio_email AS VARCHAR(32);
    SET @dominio_email = '@edu.sme.prefeitura.sp.gov.br';

    -- Extrai as palavras no nome do aluno e a quantidade
    INSERt into @palavras_nomes 
        SELECT ROW_NUMBER() OVER (ORDER BY (select null)) as id, elemento 
        FROM proc_string_split(@p_nm_aluno, ' ');
        
    SET @ultimo_indice_palavras = (SELECT COUNT(id) FROM @palavras_nomes);

    SET @p_nm_aluno = '';
    SELECT @p_nm_aluno = COALESCE(@p_nm_aluno + (
        CASE
            WHEN id = 1 OR id = @ultimo_indice_palavras THEN elemento
            ELSE CASE 
                WHEN LOWER(elemento) IN ('da', 'das', 'de', 'do', 'dos', 'os') THEN ''
                WHEN LEN(elemento) > 1 THEN LEFT(elemento, 1)
                ELSE elemento
            end 
        END
    ), elemento) FROM @palavras_nomes;
        
    -- Extrai e formata a data de nascimento
    SET @str_dt_nascimento = (
        RIGHT('00' + LTRIM(CAST(DATEPART(DAY, @p_dt_nascimento_aluno) as NVARCHAR(2))), 2) + 
        RIGHT('00' + LTRIM(CAST(DATEPART(MONTH, @p_dt_nascimento_aluno) as NVARCHAR(2))), 2) + 
        RIGHT('0000' + LTRIM(CAST(DATEPART(YEAR, @p_dt_nascimento_aluno) as NVARCHAR(4))), 4)
    );

    -- Monta o email
    SET @nm_email = LOWER(@p_nm_aluno + '.' + @str_dt_nascimento + @dominio_email);
    RETURN @nm_email;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[proc_gerar_email_docente_unidade_parceira]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 
-- Function para geracao do email
--
CREATE FUNCTION [dbo].[proc_gerar_email_docente_unidade_parceira](@p_nm_docente as VARCHAR(128), @p_cpf_docente AS VARCHAR(10))
RETURNS VARCHAR(128)
BEGIN
    DECLARE @palavras_nomes TABLE( id INT, elemento VARCHAR(64) );
    DECLARE @nm_email AS VARCHAR(128);
    
    DECLARE @dominio_email AS VARCHAR(32);
    SET @dominio_email = '@edu.sme.prefeitura.sp.gov.br';

    -- Extrai as palavras no nome do aluno e a quantidade
    INSERt into @palavras_nomes 
        SELECT ROW_NUMBER() OVER (ORDER BY (select null)) as id, elemento 
        FROM [dbo].[proc_string_split](@p_nm_docente, ' ');
        
	DECLARE @primeiro_nome as VARCHAR(50);
    
	SET @primeiro_nome = (SELECT TOP 1 elemento FROM @palavras_nomes ORDER BY id);

	DECLARE @ultimo_nome as VARCHAR(50);
    
	SET @ultimo_nome = (SELECT TOP 1 elemento FROM @palavras_nomes ORDER BY id DESC);
    
    -- Monta o email
    SET @nm_email = LOWER(@primeiro_nome +  @ultimo_nome + '.' + @p_cpf_docente + @dominio_email);
    RETURN @nm_email;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[proc_gerar_email_funcionario]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- 
-- Function para geracao do email
--
CREATE FUNCTION [dbo].[proc_gerar_email_funcionario](@p_nm_funcionario as VARCHAR(128), @p_rf_funcionario AS VARCHAR(10))
RETURNS VARCHAR(128)
BEGIN
    DECLARE @palavras_nomes TABLE( id INT, elemento VARCHAR(64) );
    DECLARE @nm_email AS VARCHAR(128);
    
    DECLARE @dominio_email AS VARCHAR(32);
    SET @dominio_email = '@edu.sme.prefeitura.sp.gov.br';

    -- Extrai as palavras no nome do aluno e a quantidade
    INSERt into @palavras_nomes 
        SELECT ROW_NUMBER() OVER (ORDER BY (select null)) as id, elemento 
        FROM [dbo].[proc_string_split](@p_nm_funcionario, ' ');
        
	DECLARE @primeiro_nome as VARCHAR(50);
    
	SET @primeiro_nome = (SELECT TOP 1 elemento FROM @palavras_nomes ORDER BY id);

	DECLARE @ultimo_nome as VARCHAR(50);
    
	SET @ultimo_nome = (SELECT TOP 1 elemento FROM @palavras_nomes ORDER BY id DESC);
    
    -- Monta o email
    SET @nm_email = LOWER(@primeiro_nome +  @ultimo_nome + '.' + @p_rf_funcionario + @dominio_email);
    RETURN @nm_email;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[proc_gerar_password_aluno]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[proc_gerar_password_aluno]
(
	@p_dt_nascimento_aluno AS DATETIME
)
RETURNS VARCHAR(8)
AS
BEGIN
	DECLARE @passwordAluno AS VARCHAR(8);
	DECLARE @dia AS VARCHAR(3) = '0' + CAST(DAY(@p_dt_nascimento_aluno) AS VARCHAR);
	DECLARE @mes AS VARCHAR(3) = '0' + CAST(MONTH(@p_dt_nascimento_aluno) AS VARCHAR);
	DECLARE @ano AS VARCHAR(4) = CAST(YEAR(@p_dt_nascimento_aluno) AS VARCHAR);

	SET @passwordAluno = (SELECT RIGHT(@dia, 2) + RIGHT(@mes, 2) + @ano);
	RETURN @passwordAluno

END
GO
/****** Object:  UserDefinedFunction [dbo].[proc_gerar_unidade_organizacional_aluno]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[proc_gerar_unidade_organizacional_aluno]
(
	@p_cd_modalidade_ensino AS INT,
	@p_cd_etapa_ensino AS INT,
	@p_cd_ciclo_ensino AS INT,
	@p_tp_escola AS INT
)
RETURNS VARCHAR(50)
AS
BEGIN
	DECLARE @unidade_organizacional AS VARCHAR(50) =
	(
		SELECT
			CASE
				WHEN @p_tp_escola = 23 THEN '/Alunos/PROFISSIONAL'

				WHEN (@p_cd_modalidade_ensino = 1 AND @p_cd_etapa_ensino = 1 AND @p_cd_ciclo_ensino = 1) OR
					 (@p_cd_modalidade_ensino = 2 AND @p_cd_etapa_ensino = 10 AND @p_cd_ciclo_ensino = 14) THEN '/Alunos/INFANTIL I'

				WHEN (@p_cd_modalidade_ensino = 1 AND @p_cd_etapa_ensino = 1 AND @p_cd_ciclo_ensino = 2) OR
					 (@p_cd_modalidade_ensino = 2 AND @p_cd_etapa_ensino = 10 AND @p_cd_ciclo_ensino = 23) THEN '/Alunos/INFANTIL II'

				WHEN (@p_cd_modalidade_ensino = 1 AND @p_cd_etapa_ensino = 2 AND @p_cd_ciclo_ensino IN (3, 4)) OR
					 (@p_cd_modalidade_ensino = 1 AND @p_cd_etapa_ensino = 3 AND @p_cd_ciclo_ensino IN (5, 6)) OR
					 (@p_cd_modalidade_ensino = 2 AND @p_cd_etapa_ensino = 11 AND @p_cd_ciclo_ensino IN (15,16)) THEN '/Alunos/EJA'

				WHEN (@p_cd_modalidade_ensino = 1 AND @p_cd_etapa_ensino = 6 AND @p_cd_ciclo_ensino = 10) OR
					 (@p_cd_modalidade_ensino = 1 AND @p_cd_etapa_ensino = 9 AND @p_cd_ciclo_ensino = 13) OR 
					 (@p_cd_modalidade_ensino = 3 AND @p_cd_etapa_ensino = 14 AND @p_cd_ciclo_ensino = 20) OR 
					 (@p_cd_modalidade_ensino = 2 AND @p_cd_etapa_ensino = 17 AND @p_cd_ciclo_ensino = 32) THEN '/Alunos/MEDIO'

				ELSE '/Alunos/FUNDAMENTAL'
			END
	);

	RETURN @unidade_organizacional

END
GO
/****** Object:  UserDefinedFunction [dbo].[proc_gerar_unidade_organizacional_funcionario]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[proc_gerar_unidade_organizacional_funcionario](@p_rf_funcionario as VARCHAR(10))
RETURNS VARCHAR(50)
BEGIN
	DECLARE @codigo_servidor INT;

	SET @codigo_servidor = (select
		cd_servidor as codigo_servidor
	from
		v_servidor_cotic
	where
		cd_registro_funcional is not null
		and cd_registro_funcional = @p_rf_funcionario);

	declare @Usuario TABLE 
	( 
		CodigoRf CHAR(7) ,
		Nome VARCHAR(60),
		Cpf VARCHAR(11),
		CodigoServidor INT,
		 TipoLaudo CHAR(1),
		 DataCessacaoLaudo DATETIME,
		 CargoId INT,
		CodigoCargoSobrePostoReferencia INT,
		 IdCargoBase INT,
         CodigoComponenteCurricular INT,
		 TipoEscolaBase INT,
		DataAtribuicao DATETIME,
		CargoBase INT,
		TipoUnidadeEducacaoBase INT,
		CargoSobrePostoId INT,
		IdCargoSobreposto INT,
		TipoEscolaSobreposto INT ,
		Codigo INT,
		TipoUnidadeEducacao INT,
		CodigoFuncaoAtividade INT,
		TipoEscolaFuncaoAtividade INT,
		TipoUnidadeEducacaoFuncaoAtividade INT
	)

		INSERT INTO @Usuario
		select distinct
				   Rf as CodigoRf,
				   Nome,
				   Cpf,
				   CodigoServidor,
				   laudo.cd_tipo_laudo                    as TipoLaudo,
				   laudo.dt_publicacao_doc_cessacao_laudo as DataCessacaoLaudo,
				   CdCargoBaseServidor                    as CargoId,
				   CargoSobre							  as CodigoCargoSobrePostoReferencia,
				   CdCargoBaseServidor                    as IdCargoBase,
                   atb.cd_componente_curricular           as CodigoComponenteCurricular,
				   coalesce(ue.tp_escola,TipoEscolaBase)  as TipoEscolaBase,
				   COALESCE(atb.dt_atribuicao_aula,Atribuicao) as DataAtribuicao,
				   CargoBase,
				   TipoUnidadeEducacaoBase,
				   CdCargoSobrepostoServidor              as CargoSobrePostoId,
				   CdCargoSobrepostoServidor              as IdCargoSobreposto,
				   TipoEscolaSobre                        as TipoEscolaSobreposto,
				   CargoSobre                             as Codigo,
				   TipoUnidadeEducacaoSobre               as TipoUnidadeEducacao,
				   CdTipoFuncaoAtividade                  as CodigoFuncaoAtividade,
				   TipoEscolaFuncaoAtividade,
				   TipoUnidadeEducacaoFuncaoAtividade
			         from (
                        select sev.cd_registro_funcional        AS Rf,
                            sev.nm_pessoa                    AS Nome,
                            sev.cd_cpf_pessoa                AS Cpf,
                            cba.cd_cargo_base_servidor       as CdCargoBaseServidor,
                            esc_base.tp_escola               AS TipoEscolaBase,
                            cba.cd_cargo                     AS CargoBase,
                            ue_base.tp_unidade_educacao      AS TipoUnidadeEducacaoBase,
                            css.cd_cargo_sobreposto_servidor as CdCargoSobrepostoServidor,
                            esc_sobre.tp_escola              as TipoEscolaSobre,
                            css.cd_cargo                     AS CargoSobre,
                            ue_sobre.tp_unidade_educacao     as TipoUnidadeEducacaoSobre,
                            esc_fat.tp_escola                as TipoEscolaFuncaoAtividade,
                            fat.cd_tipo_funcao               as CdTipoFuncaoAtividade,
                            ue_fat.tp_unidade_educacao       as TipoUnidadeEducacaoFuncaoAtividade,
							sev.cd_servidor                  as CodigoServidor,
							COALESCE(ls.dt_fim, fat.dt_fim_funcao_atividade) as Atribuicao
                        from se1426.dbo.v_servidor_cotic sev with (nolock)
                                -- Cargo Base
                                inner join v_cargo_base_cotic cba with (nolock) on sev.cd_servidor = cba.cd_servidor
                                left join lotacao_servidor ls with (nolock)
                                        on cba.cd_cargo_base_servidor = ls.cd_cargo_base_servidor and (ls.dt_fim is null  or cba.dt_fim_nomeacao > '2020-07-01')
                            -- Cargo Sobreposto
                                left join cargo_sobreposto_servidor css with (nolock)
                                        on cba.cd_cargo_base_servidor = css.cd_cargo_base_servidor AND
                                            (css.dt_fim_cargo_sobreposto IS NULL OR
                                            css.dt_fim_cargo_sobreposto > '2020-07-01')
                            -- Funcao Atividade
                                left join funcao_atividade_cargo_servidor fat with (nolock)
                                        on fat.cd_cargo_base_servidor = cba.cd_cargo_base_servidor
                                            and (fat.dt_cancelamento is null or dt_fim_funcao_atividade > '2020-07-01')
                                            and  fat.dt_fim_funcao_atividade is null
                            --Unidades
                                left join v_cadastro_unidade_educacao ue_base with (nolock)
                                        on (ls.cd_unidade_educacao = ue_base.cd_unidade_educacao)
                                left join escola esc_base with (nolock) on esc_base.cd_escola = ls.cd_unidade_educacao
                                left join v_cadastro_unidade_educacao ue_sobre with (nolock)
                                        on (css.cd_unidade_local_servico = ue_sobre.cd_unidade_educacao)
                                left join escola esc_sobre with (nolock) on esc_sobre.cd_escola = ue_sobre.cd_unidade_educacao
                                left join v_cadastro_unidade_educacao ue_fat with (nolock)
                                        on (fat.cd_unidade_local_servico = ue_fat.cd_unidade_educacao)
                                left join escola esc_fat with (nolock) on esc_fat.cd_escola = fat.cd_unidade_local_servico
                        where
                            sev.cd_servidor = @codigo_servidor
							and (cba.dt_fim_nomeacao is null or cba.dt_fim_nomeacao > '2020-07-01')
							and (cba.dt_cancelamento is null  or cba.dt_cancelamento > '2020-07-01')) servidor
                        -- Atribuicao
                        left join (
                select distinct cd_componente_curricular, cd_cargo_base_servidor, dt_atribuicao_aula, cd_unidade_educacao
                from atribuicao_aula atb with (nolock)
                where atb.an_atribuicao = year(getdate())
                    and (atb.dt_cancelamento is null or atb.dt_cancelamento > '2020-07-01')
                    and atb.dt_disponibilizacao_aulas is null
            ) atb on atb.cd_cargo_base_servidor = servidor.CdCargoBaseServidor
                -- laudo
                        left join (select cd_cargo_base_servidor,
                                        cd_laudo_medico,
                                        row_number()
                                                over (partition by cd_cargo_base_servidor order by cd_laudo_medico desc ) ordem,
                                        cd_tipo_laudo,
                                        dt_inicio,
                                        dt_publicacao_doc_cessacao_laudo
                                from laudo_medico with (nolock)
                                where dt_publicacao_doc_cessacao_laudo is null) laudo
                                on laudo.cd_cargo_base_servidor = servidor.CdCargoBaseServidor and ordem = 1
						left join escola ue on ue.cd_escola = atb.cd_unidade_educacao;

						DECLARE @componente_cj_infantil INT;
						SET @componente_cj_infantil = 504;

						DECLARE @cargo_CP INT;
						SET @cargo_CP = 3379;

						DECLARE @cargo_AD INT;
						SET @cargo_AD = 3085;

						DECLARE @cargo_diretor INT;
						SET @cargo_diretor = 3360;

						DECLARE @cargo_supervisor INT;
						SET @cargo_supervisor = 3352;

						DECLARE @cargo_ATE INT;
						SET @cargo_ATE = 4906;

						declare @CargosSupervisorTecnico TABLE (Id INT,Elemento VARCHAR(10))

						declare @ComponentesCJ TABLE (Id INT,Elemento VARCHAR(10))

						declare @CargosProfessor TABLE (Id INT,Elemento VARCHAR(10))

						INSERT INTO @CargosSupervisorTecnico
						SELECT ROW_NUMBER() OVER (ORDER BY (select null)) as id, elemento
								FROM dbo.proc_string_split('433,433', ',');

						INSERT INTO @ComponentesCJ
						SELECT ROW_NUMBER() OVER (ORDER BY (select null)) as id, elemento
								FROM dbo.proc_string_split('514,526,527,528,529,530,531,532,533', ',');
						
						INSERT INTO @CargosProfessor
						SELECT ROW_NUMBER() OVER (ORDER BY (select null)) as id, elemento
								FROM dbo.proc_string_split('3131,3212,3213,3220,3239,3247,3255,3263,3271,3280,3298,3301,3310,3336,3344,3395,3425,3433,3450,3816,3840,3859,3867,3874,3875,3877,3880,3883,3884', ',');
						
						DECLARE @cargo_base_servidor INT;
						SET @cargo_base_servidor = (SELECT TOP 1 CargoBase from @Usuario);

						DECLARE @cargo_sobreposto_servidor INT;
						SET @cargo_sobreposto_servidor = (SELECT TOP 1 CodigoCargoSobrePostoReferencia from @Usuario);

		IF @cargo_base_servidor = @cargo_CP OR  @cargo_sobreposto_servidor = @cargo_CP
			RETURN '/Admin/CP'
		ELSE IF @cargo_base_servidor = @cargo_AD OR @cargo_sobreposto_servidor = @cargo_AD
			RETURN '/Admin/AD'
		ELSE IF @cargo_base_servidor = @cargo_diretor OR @cargo_sobreposto_servidor = @cargo_diretor
			RETURN '/Admin/DIRETOR'
		ELSE IF @cargo_base_servidor = @cargo_ATE OR @cargo_sobreposto_servidor = @cargo_ATE
			RETURN '/Professor/ATE'
		ELSE IF EXISTS(SELECT 1 FROM @CargosSupervisorTecnico WHERE elemento in  (@cargo_base_servidor, @cargo_sobreposto_servidor))
			 OR @cargo_base_servidor = @cargo_supervisor OR @cargo_sobreposto_servidor = @cargo_supervisor
			RETURN '/Admin/Supervisores'
		ELSE IF EXISTS(SELECT * FROM @CargosProfessor WHERE elemento in  (@cargo_base_servidor, @cargo_sobreposto_servidor))
		BEGIN
			DECLARE @tipo_laudo CHAR(1);
			SET @tipo_laudo = (SELECT TOP 1 TipoLaudo from @Usuario);

			DECLARE @data_laudo DATETIME;
			SET @data_laudo = (SELECT TOP 1 DataCessacaoLaudo from @Usuario);

			IF UPPER(@tipo_laudo) = 'R' AND (@data_laudo IS NULL OR @data_laudo > GETDATE())
				RETURN '/Professores/Readaptados'
			ELSE
			BEGIN
				DECLARE @componente_curricular INT;
				SET @componente_curricular = (SELECT TOP 1 CodigoComponenteCurricular from @Usuario);

				IF @componente_curricular = @componente_cj_infantil OR 
					EXISTS(SELECT 1 FROM @ComponentesCJ WHERE elemento = @componente_curricular)
					RETURN ' /Professores/Sem Atribuição'
				ELSE
					RETURN '/Professores'
			END;
		END;

		RETURN NULL;
END
GO
/****** Object:  UserDefinedFunction [dbo].[proc_gerar_unidade_organizacional_funcionario_v2]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[proc_gerar_unidade_organizacional_funcionario_v2](@p_cd_cargo AS INT, @p_tipo_laudo AS CHAR)
RETURNS VARCHAR(50)
BEGIN
	DECLARE @unidade_organizacional AS VARCHAR(50) =
	(
		SELECT
			CASE
				WHEN @p_cd_cargo = 3379 THEN '/Admin/CP'
				WHEN @p_cd_cargo = 3085 THEN '/Admin/AD'
				WHEN @p_cd_cargo = 3360 THEN '/Admin/DIRETOR'
				WHEN @p_cd_cargo = 3352 OR @p_cd_cargo = 433 OR @p_cd_cargo = 433 THEN '/Admin/Supervisores'
				WHEN @p_cd_cargo = 3352 THEN '/Admin/Supervisores'
				WHEN @p_cd_cargo = 4906 THEN '/Professores/ATE'
				WHEN @p_tipo_laudo = 'R' THEN '/Professores/Readaptados'

				ELSE '/Professores'
			END
	);

	RETURN @unidade_organizacional
END
GO
/****** Object:  UserDefinedFunction [dbo].[proc_obter_func_cargo_ue]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[proc_obter_func_cargo_ue] (@p_cod_ue as varchar(10))
RETURNS @retFuncCargoUe TABLE
(
    NomeServidor varchar(70) NOT NULL,
    CodigoRf varchar(7) NOT NULL,
	DataInicio datetime NULL,
	DataFim datetime NULL,
    Cargo varchar(50) NULL,
	CodigoCargo int null
)
AS
BEGIN
WITH FCUE_cte(NomeServidor, CodigoRf, DataInicio, DataFim, Cargo, CodigoCargo) -- CTE name and columns
    AS (
      SELECT 
	            NomeServidor
	            ,CodigoRF
				,DataInicio 
			    ,DataFim  
	            ,Cargo
				,CodigoCargo
	            FROM(
	                SELECT DISTINCT 
							nm_pessoa              NomeServidor 
			                ,cd_registro_funcional            CodigoRF 
			                ,cargoServidor.dt_posse           DataInicio 
			                ,cargoServidor.dt_fim_nomeacao    DataFim  
	                        ,CASE WHEN cargoSobreposto.dc_cargo IS NOT NULL THEN cargoSobreposto.dc_cargo ELSE cargo.dc_cargo END AS Cargo
				            ,CASE WHEN cargoSobreposto.cd_cargo IS NOT NULL THEN cargoSobreposto.cd_cargo ELSE cargo.cd_cargo END AS CodigoCargo
	                FROM v_servidor_cotic servidor
	                INNER JOIN v_cargo_base_cotic AS cargoServidor ON cargoServidor.CD_SERVIDOR = servidor.cd_servidor
	                INNER JOIN cargo AS cargo ON cargoServidor.cd_cargo = cargo.cd_cargo
	                LEFT JOIN lotacao_servidor AS lotacao_servidor 
		                ON cargoServidor.cd_cargo_base_servidor = lotacao_servidor.cd_cargo_base_servidor
	                INNER JOIN v_cadastro_unidade_educacao dre 
		                ON lotacao_servidor.cd_unidade_educacao = dre.cd_unidade_educacao 
		            LEFT JOIN (
			            SELECT 
				             cargoSobreposto.cd_cargo
				            ,cargoSobreposto.dc_cargo
				            ,cargo_sobreposto_servidor.cd_cargo_base_servidor 
				            ,cargo_sobreposto_servidor.cd_unidade_local_servico
			            FROM cargo_sobreposto_servidor AS cargo_sobreposto_servidor 
				            INNER JOIN cargo AS cargoSobreposto ON cargo_sobreposto_servidor.cd_cargo = cargoSobreposto.cd_cargo
				            INNER JOIN lotacao_servidor AS lotacao_servidor_sobreposto 
					            ON cargo_sobreposto_servidor.cd_cargo_base_servidor = lotacao_servidor_sobreposto.cd_cargo_base_servidor
			            WHERE (cargo_sobreposto_servidor.dt_fim_cargo_sobreposto IS NULL
			            OR cargo_sobreposto_servidor.dt_fim_cargo_sobreposto > GETDATE())) cargoSobreposto
				            ON cargoSobreposto.cd_cargo_base_servidor = cargoServidor.cd_cargo_base_servidor
					            AND cargoSobreposto.cd_unidade_local_servico = dre.cd_unidade_educacao
	                WHERE  lotacao_servidor.dt_fim IS NULL AND dre.cd_unidade_educacao = @p_cod_ue
		            UNION
		            SELECT DISTINCT nm_pessoa              NomeServidor 
			                ,cd_registro_funcional            CodigoRF 
			                ,cargoServidor.dt_posse           DataInicio 
			                ,cargoServidor.dt_fim_nomeacao    DataFim  
			                ,RTRIM(LTRIM(cargo.dc_cargo))     Cargo
			                ,cargo.cd_cargo					  CodigoCargo
	                FROM v_servidor_cotic servidor
		                INNER JOIN v_cargo_base_cotic AS cargoServidor ON cargoServidor.CD_SERVIDOR = servidor.cd_servidor
		                LEFT JOIN lotacao_servidor AS lotacao_servidor ON cargoServidor.cd_cargo_base_servidor = lotacao_servidor.cd_cargo_base_servidor
		                INNER JOIN cargo_sobreposto_servidor AS cargo_sobreposto_servidor 
			                ON cargo_sobreposto_servidor.cd_cargo_base_servidor = cargoServidor.cd_cargo_base_servidor
			                AND (cargo_sobreposto_servidor.dt_fim_cargo_sobreposto IS NULL
			                OR cargo_sobreposto_servidor.dt_fim_cargo_sobreposto > GETDATE())
                        INNER JOIN cargo AS cargo ON cargo_sobreposto_servidor.cd_cargo = cargo.cd_cargo
		                INNER JOIN v_cadastro_unidade_educacao dre 
			                ON cargo_sobreposto_servidor.cd_unidade_local_servico = dre.cd_unidade_educacao
		            WHERE  lotacao_servidor.dt_fim IS NULL AND dre.cd_unidade_educacao = @p_cod_ue
                           AND  cargoServidor.dt_fim_nomeacao IS NULL
	                UNION
	                SELECT DISTINCT nm_pessoa              NomeServidor 
			                ,cd_registro_funcional            CodigoRF 
			                ,cargoServidor.dt_posse           DataInicio 
			                ,cargoServidor.dt_fim_nomeacao    DataFim  
			                ,RTRIM(LTRIM(cargo.dc_cargo))     Cargo
			                ,cargo.cd_cargo					  CodigoCargo
	                FROM v_servidor_cotic servidor
		                INNER JOIN v_cargo_base_cotic AS cargoServidor ON cargoServidor.CD_SERVIDOR = servidor.cd_servidor
		                INNER JOIN cargo AS cargo ON cargoServidor.cd_cargo = cargo.cd_cargo
		                INNER JOIN atribuicao_aula atribuicao ON atribuicao.cd_cargo_base_servidor = cargoServidor.cd_cargo_base_servidor
		                INNER JOIN v_cadastro_unidade_educacao dre 
			                ON atribuicao.cd_unidade_educacao = dre.cd_unidade_educacao
	                WHERE   atribuicao.dt_cancelamento IS NULL 
		                AND dre.cd_unidade_educacao = @p_cod_ue 
		                AND cargoServidor.dt_fim_nomeacao IS NULL
		                AND atribuicao.dt_disponibilizacao_aulas IS  NULL
		                AND YEAR(dt_atribuicao_aula)  = YEAR(GETDATE())                        
	                UNION
		                SELECT DISTINCT nm_pessoa              NomeServidor 
			                ,cd_registro_funcional            CodigoRF 
			                ,cargoServidor.dt_posse           DataInicio 
			                ,cargoServidor.dt_fim_nomeacao    DataFim  
			                ,RTRIM(LTRIM(cargo.dc_cargo))     Cargo
			                ,cargo.cd_cargo					  CodigoCargo
	                FROM v_servidor_cotic servidor
		                INNER JOIN v_cargo_base_cotic AS cargoServidor ON cargoServidor.CD_SERVIDOR = servidor.cd_servidor
		                INNER JOIN cargo AS cargo ON cargoServidor.cd_cargo = cargo.cd_cargo
		                INNER JOIN funcao_atividade_cargo_servidor atividade ON atividade.cd_cargo_base_servidor = cargoServidor.cd_cargo_base_servidor
		                INNER JOIN v_cadastro_unidade_educacao dre 
			                ON atividade.cd_unidade_local_servico = dre.cd_unidade_educacao
	                WHERE   atividade.dt_fim_funcao_atividade IS NULL 
		                AND dre.cd_unidade_educacao = @p_cod_ue 
                        AND cargoServidor.dt_fim_nomeacao IS NULL
            ) Funcionarios
			 WHERE Funcionarios.DataFim is null and CodigoCargo in (3379, 3085, 3360, 3352, 433, 42, 43, 44)
        )
-- copy the required columns to the result of the function
    INSERT @retFuncCargoUe
    SELECT  NomeServidor, CodigoRF, DataInicio, DataFim, Cargo, CodigoCargo
    FROM FCUE_cte
    RETURN
END;
GO
/****** Object:  UserDefinedFunction [dbo].[proc_obter_func_funcao_ue]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[proc_obter_func_funcao_ue] (@p_cod_ue as varchar(10))
RETURNS @retFuncFuncaoUe TABLE
(
    NomeServidor varchar(70) NOT NULL,
    CodigoRf varchar(7) NOT NULL,
	DataInicio datetime NULL,
	DataFim datetime NULL,
    CdTipoFuncao int NULL
)
AS
BEGIN
WITH FFUE_cte(NomeServidor, CodigoRf, DataInicio, DataFim, CdTipoFuncao)
    AS (
		SELECT DISTINCT	 
					servidor.nm_pessoa as NomeServidor,
					servidor.cd_registro_funcional as CodigoRf,
	                cargobase.dt_posse           DataInicio, 
			        cargobase.dt_fim_nomeacao    DataFim,
					funcao.cd_tipo_funcao CdTipoFuncao
                FROM v_servidor_cotic servidor
                INNER JOIN v_cargo_base_cotic cargobase ON servidor.cd_servidor = cargobase.cd_servidor
                INNER JOIN funcao_atividade_cargo_servidor funcao ON cargobase.cd_cargo_base_servidor = funcao.cd_cargo_base_servidor
                INNER JOIN v_cadastro_unidade_educacao ue ON funcao.cd_unidade_local_servico = ue.cd_unidade_educacao
                INNER JOIN v_cadastro_unidade_educacao dre ON dre.cd_unidade_educacao = ue.cd_unidade_administrativa_referencia
                INNER JOIN escola ON ue.cd_unidade_educacao = escola.cd_escola
                INNER JOIN tipo_unidade_educacao tue ON ue.tp_unidade_educacao  = tue.tp_unidade_educacao
                INNER JOIN tipo_escola ON escola.tp_escola = tipo_escola.tp_escola
                INNER JOIN unidade_administrativa 
	                ON ue.cd_unidade_administrativa_referencia = 
					                unidade_administrativa.cd_unidade_administrativa 
		                AND tp_unidade_administrativa = 24 
                WHERE 
					(funcao.dt_fim_funcao_atividade IS NULL OR funcao.dt_fim_funcao_atividade > GETDATE())
	                AND cargobase.dt_cancelamento IS NULL 
	                AND cargobase.dt_fim_nomeacao IS NULL
	                AND escola.tp_escola IN (13)
					AND funcao.cd_tipo_funcao in (42,43,44)
					AND escola.cd_escola = @p_cod_ue
			   )
-- copy the required columns to the result of the function
    INSERT @retFuncFuncaoUe
    SELECT  NomeServidor, CodigoRF, DataInicio, DataFim, CdTipoFuncao
    FROM FFUE_cte
    RETURN
END;
GO
/****** Object:  UserDefinedFunction [dbo].[proc_obter_nivel]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[proc_obter_nivel] (@p_cod_servidor as int, @p_cod_ue as varchar(10))
RETURNS @retFuncCargoUe TABLE
(
    NomeServidor varchar(70) NOT NULL,
    CodigoRf varchar(7) NOT NULL,
	DataInicio datetime NULL,
	DataFim datetime NULL,
    Cargo varchar(50) NULL,
	CodigoCargo int null
)
BEGIN
	IF @p_cod_servidor IS NOT NULL AND @p_cod_servidor <> '' AND @p_cod_servidor > 0
		RETURN;
	ELSE
		BEGIN 

			DECLARE @cd_cargo_CP INT = 3379;
			DECLARE @cd_cargo_AD INT = 3085;
			DECLARE @cd_cargo_DIRETOR INT = 3360;
			DECLARE @cd_cargo_SUPERVISOR1 INT = 3352 
			DECLARE @cd_cargo_SUPERVISOR2 INT = 433;
			DECLARE @cd_cargo_CIEJAASSISTPED INT = 44;
			DECLARE @cd_cargo_CIEJAASSISTCOORD INT = 43;
			DECLARE @cd_cargo_CIEJACOORD INT = 42;

			DECLARE @tbFuncCargosUe TABLE
			(
				NomeServidor varchar(70) NOT NULL,
				CodigoRf varchar(7) NOT NULL,
				DataInicio datetime NULL,
				DataFim datetime NULL,
				Cargo varchar(50) NULL,
				CodigoCargo int NULL
			)

			insert into @tbFuncCargosUe
			select NomeServidor,CodigoRf,DataInicio,DataFim,Cargo,CodigoCargo FROM [dbo].[proc_obter_func_cargo_ue](@p_cod_ue);

			IF EXISTS (select 1 from @tbFuncCargosUe where CodigoCargo = @cd_cargo_CP)
			BEGIN
				INSERT INTO @retFuncCargoUe
				SELECT TOP 1 NomeServidor,CodigoRf,DataInicio,DataFim,Cargo,CodigoCargo FROM @tbFuncCargosUe WHERE CodigoCargo = @cd_cargo_CP;
				RETURN;
			END

			IF EXISTS (select 1 from @tbFuncCargosUe where CodigoCargo = @cd_cargo_AD)
			BEGIN
				INSERT INTO @retFuncCargoUe
				SELECT TOP 1 NomeServidor,CodigoRf,DataInicio,DataFim,Cargo,CodigoCargo FROM @tbFuncCargosUe WHERE CodigoCargo = @cd_cargo_AD;
				RETURN;
			END

			IF EXISTS (select 1 from @tbFuncCargosUe where CodigoCargo = @cd_cargo_DIRETOR)
			BEGIN
				INSERT INTO @retFuncCargoUe
				SELECT TOP 1 NomeServidor,CodigoRf,DataInicio,DataFim,Cargo,CodigoCargo FROM @tbFuncCargosUe WHERE CodigoCargo = @cd_cargo_DIRETOR;
				RETURN;
			END

			IF EXISTS (select 1 from @tbFuncCargosUe where CodigoCargo = @cd_cargo_SUPERVISOR1)
			BEGIN
				INSERT INTO @retFuncCargoUe
				SELECT TOP 1 NomeServidor,CodigoRf,DataInicio,DataFim,Cargo,CodigoCargo FROM @tbFuncCargosUe WHERE CodigoCargo = @cd_cargo_SUPERVISOR1;
				RETURN;
			END

			IF EXISTS (select 1 from @tbFuncCargosUe where CodigoCargo = @cd_cargo_SUPERVISOR2)
			BEGIN
				INSERT INTO @retFuncCargoUe
				SELECT TOP 1 NomeServidor,CodigoRf,DataInicio,DataFim,Cargo,CodigoCargo FROM @tbFuncCargosUe WHERE CodigoCargo = @cd_cargo_SUPERVISOR2;
				RETURN;
			END

			DECLARE @tbFuncFuncaoUe TABLE
			(
				NomeServidor varchar(70) NOT NULL,
				CodigoRf varchar(7) NOT NULL,
				DataInicio datetime NULL,
				DataFim datetime NULL,
				CdTipoFuncao int NULL
			)

			insert into @tbFuncFuncaoUe
			select NomeServidor,CodigoRf,DataInicio,DataFim,CdTipoFuncao FROM [dbo].[proc_obter_func_funcao_ue](@p_cod_ue);

			IF EXISTS (select 1 from @tbFuncFuncaoUe where CdTipoFuncao = @cd_cargo_CIEJAASSISTPED)
			BEGIN
				INSERT INTO @retFuncCargoUe
				SELECT TOP 1 NomeServidor,CodigoRf,DataInicio,DataFim,null,CdTipoFuncao FROM @tbFuncFuncaoUe WHERE CdTipoFuncao = @cd_cargo_CIEJAASSISTPED;
				RETURN;
			END

			IF EXISTS (select 1 from @tbFuncFuncaoUe where CdTipoFuncao = @cd_cargo_CIEJAASSISTCOORD)
			BEGIN
				INSERT INTO @retFuncCargoUe
				SELECT TOP 1 NomeServidor,CodigoRf,DataInicio,DataFim,null,CdTipoFuncao FROM @tbFuncFuncaoUe WHERE CdTipoFuncao = @cd_cargo_CIEJAASSISTCOORD;
				RETURN;
			END

			IF EXISTS (select 1 from @tbFuncFuncaoUe where CdTipoFuncao = @cd_cargo_CIEJACOORD)
			BEGIN
				INSERT INTO @retFuncCargoUe
				SELECT TOP 1 NomeServidor,CodigoRf,DataInicio,DataFim,null,CdTipoFuncao FROM @tbFuncFuncaoUe WHERE CdTipoFuncao = @cd_cargo_CIEJACOORD;
				RETURN;
			END
			
		END

	RETURN
END
GO
/****** Object:  UserDefinedFunction [dbo].[proc_remove_acentuacao]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[proc_remove_acentuacao](
    @String VARCHAR(MAX)
)
RETURNS VARCHAR(MAX)
AS
BEGIN
    
    /****************************************************************************************************************/
    /** RETIRA ACENTUAÇÃO DAS VOGAIS **/
    /****************************************************************************************************************/
    SET @String = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@String,'á','a'),'à','a'),'â','a'),'ã','a'),'ä','a')
    SET @String = REPLACE(REPLACE(REPLACE(REPLACE(@String,'é','e'),'è','e'),'ê','e'),'ë','e')
    SET @String = REPLACE(REPLACE(REPLACE(REPLACE(@String,'í','i'),'ì','i'),'î','i'),'ï','i')
    SET @String = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@String,'ó','o'),'ò','o'),'ô','o'),'õ','o'),'ö','o')
    SET @String = REPLACE(REPLACE(REPLACE(REPLACE(@String,'ú','u'),'ù','u'),'û','u'),'ü','u')
    
    /****************************************************************************************************************/
    /** RETIRA ACENTUAÇÃO DAS CONSOANTES **/
    /****************************************************************************************************************/
    SET @String = REPLACE(@String,'ý','y')
    SET @String = REPLACE(@String,'ñ','n')
    SET @String = REPLACE(@String,'ç','c')
    
    RETURN @String
 
END
GO
/****** Object:  UserDefinedFunction [dbo].[proc_remover_caracteres_especias]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[proc_remover_caracteres_especias](@in varchar(MAX))
   RETURNS NVarChar(MAX)
AS
BEGIN
   DECLARE @out NVarChar(MAX), @i int, @c int, @c2 int, @c3 int, @nc int

   SELECT @i = 1, @out = ''

   WHILE (@i <= Len(@in))
   BEGIN
      SET @c = Ascii(SubString(@in, @i, 1))

      IF (@c < 128)
      BEGIN
         SET @nc = @c
         SET @i = @i + 1
      END
      ELSE IF (@c > 191 AND @c < 224)
      BEGIN
         SET @c2 = Ascii(SubString(@in, @i + 1, 1))

         SET @nc = (((@c & 31) * 64 /* << 6 */) | (@c2 & 63))
         SET @i = @i + 2
      END
      ELSE
      BEGIN
         SET @c2 = Ascii(SubString(@in, @i + 1, 1))
         SET @c3 = Ascii(SubString(@in, @i + 2, 1))

         SET @nc = (((@c & 15) * 4096 /* << 12 */) | ((@c2 & 63) * 64 /* << 6 */) | (@c3 & 63))
         SET @i = @i + 3
      END

      SET @out = @out + NChar(@nc)
   END
   RETURN @out
END
GO
/****** Object:  UserDefinedFunction [dbo].[proc_replace_first]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[proc_replace_first](@X NVARCHAR(MAX), @F NVARCHAR(MAX), @R NVARCHAR(MAX)) RETURNS NVARCHAR(MAX) AS BEGIN
RETURN STUFF(@X, CHARINDEX(@F, @X), LEN(@F), @R)
END
GO
/****** Object:  UserDefinedFunction [dbo].[proc_retorna_primeiro_nome]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[proc_retorna_primeiro_nome] 
(
	@p_nome_pessoa as VARCHAR(MAX)
)
RETURNS VARCHAR(250)
AS
BEGIN
	DECLARE @primeiroNome AS VARCHAR(250) = (SELECT TOP 1 elemento FROM proc_string_split(@p_nome_pessoa, ' '));
	RETURN @primeiroNome
END
GO
/****** Object:  UserDefinedFunction [dbo].[proc_retorna_ultimo_nome]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[proc_retorna_ultimo_nome] 
(
	@p_nome_pessoa as VARCHAR(MAX)
)
RETURNS VARCHAR(250)
AS
BEGIN
	DECLARE @primeiroNome AS VARCHAR(250) = (SELECT [dbo].[proc_retorna_primeiro_nome] (@p_nome_pessoa));
	DECLARE @ultimoNome AS VARCHAR(MAX) = (SELECT [dbo].[proc_replace_first](@p_nome_pessoa, @primeiroNome, ''));
	RETURN LTRIM(@ultimoNome)
END
GO
/****** Object:  Table [dbo].[aluno]    Script Date: 03/05/2021 10:48:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[aluno](
	[cd_aluno] [int] NOT NULL,
	[nm_aluno] [varchar](70) NOT NULL,
	[dt_nascimento_aluno] [datetime] NOT NULL,
	[cd_sexo_aluno] [char](1) NOT NULL,
	[nm_mae_aluno] [varchar](70) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[tp_raca_cor] [int] NULL,
	[cd_orgao_emissor] [int] NULL,
	[cd_pais_mec] [int] NULL,
	[sg_uf_rg_aluno] [char](2) NULL,
	[dt_entrada_pais] [smalldatetime] NULL,
	[cd_distrito_cartorio_certidao] [int] NULL,
	[cd_cpf_aluno] [bigint] NULL,
	[nr_rg_aluno] [varchar](15) NULL,
	[cd_digito_rg_aluno] [char](4) NULL,
	[dt_emissao_rg] [datetime] NULL,
	[cd_identificacao_social] [char](11) NULL,
	[tp_certidao_aluno] [int] NULL,
	[nr_certidao_aluno] [char](8) NULL,
	[nr_folha_certidao_aluno] [char](4) NULL,
	[nr_livro_certidao_aluno] [char](8) NULL,
	[dt_emissao_certidao] [datetime] NULL,
	[nr_registro_estado] [varchar](15) NULL,
	[cd_digito_registro_estado] [char](4) NULL,
	[dt_atualizacao_registro_estado] [datetime] NULL,
	[in_mae_falecida] [char](1) NULL,
	[nm_pai_aluno] [varchar](70) NULL,
	[sg_uf_registro_aluno_estado] [char](2) NULL,
	[in_pai_falecido] [char](1) NULL,
	[dt_inclusao_cadastro] [datetime] NULL,
	[cd_municipio_nascimento] [int] NULL,
	[cd_municipio_certidao] [int] NULL,
	[cd_inep_aluno] [decimal](12, 0) NULL,
	[cd_matricula_certidao_aluno] [char](32) NULL,
	[cd_aluno_principal] [int] NULL,
	[dt_falecimento_aluno] [datetime] NULL,
	[cd_nacionalidade_aluno] [char](1) NOT NULL,
	[sg_uf_nascimento_aluno] [char](2) NULL,
	[nr_rne_aluno] [varchar](12) NULL,
	[in_rg_aluno_confere] [char](1) NULL,
	[in_certidao_aluno_confere] [char](1) NULL,
	[in_rne_aluno_confere] [char](1) NULL,
	[in_documento_nacional_apresentado] [char](1) NULL,
	[tx_documento_nacional_nao_apresentado] [varchar](60) NULL,
	[cd_cartorio_certidao] [int] NULL,
	[dt_exclusao] [datetime] NULL,
	[nm_social_aluno] [varchar](70) NULL,
	[cd_tipo_sigilo] [int] NULL,
	[cd_sexo_mae] [char](1) NULL,
	[cd_nacionalidade_mae] [char](1) NULL,
	[cd_pais_origem_mae] [int] NULL,
	[cd_sexo_pai] [char](1) NULL,
	[cd_nacionalidade_pai] [char](1) NULL,
	[cd_pais_origem_pai] [int] NULL,
	[in_consta_pai_documento] [char](1) NULL,
	[in_cpf_validado_receita_federal] [char](1) NULL,
	[in_autorizacao_geracao_cpf] [char](1) NULL,
 CONSTRAINT [PK_aluno] PRIMARY KEY CLUSTERED 
(
	[cd_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[aluno_turma]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[aluno_turma](
	[codigoaluno] [varchar](max) NULL,
	[nomealuno] [varchar](max) NULL,
	[turmacodigo] [varchar](max) NULL,
	[codigosituacaomatricula] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[atribuicao_aula]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[atribuicao_aula](
	[cd_atribuicao_aula] [int] NOT NULL,
	[an_atribuicao] [decimal](4, 0) NOT NULL,
	[cd_serie_grade] [int] NULL,
	[cd_turma_escola_grade_programa] [int] NULL,
	[cd_cargo_base_servidor] [int] NOT NULL,
	[cd_unidade_educacao] [char](6) NOT NULL,
	[cd_tipo_turno] [int] NOT NULL,
	[cd_grade] [int] NOT NULL,
	[cd_componente_curricular] [int] NOT NULL,
	[qt_aula_atribuida] [int] NOT NULL,
	[dt_disponibilizacao_aulas] [datetime] NULL,
	[cd_motivo_disponibilizacao] [int] NULL,
	[dt_prevista_retorno] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_cancelamento] [datetime] NULL,
	[cd_cronograma_atribuicao] [int] NULL,
	[qt_aula_atribuida_jornada_extra] [int] NULL,
	[dt_atribuicao_aula] [datetime] NOT NULL,
 CONSTRAINT [PK_atribuicao_aula] PRIMARY KEY CLUSTERED 
(
	[cd_atribuicao_aula] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[cargo]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cargo](
	[cd_cargo] [int] NOT NULL,
	[dc_cargo] [varchar](50) NULL,
	[cd_area_atuacao_cargo] [int] NOT NULL,
	[in_permissao_acumulo_cargo] [char](1) NULL,
	[in_permissao_cargo_base] [char](1) NULL,
	[in_cargo_sobreposto_comissionado] [char](1) NULL,
	[dt_cancelamento] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[in_cargo_contrato] [char](1) NULL,
	[in_opcao_jornada_trabalho] [char](1) NULL,
	[qt_carga_horaria_base] [decimal](4, 2) NULL,
	[in_aceita_funcao_atividade] [char](1) NOT NULL,
	[in_carreira_magisterio] [char](1) NULL,
 CONSTRAINT [PK_cargo] PRIMARY KEY CLUSTERED 
(
	[cd_cargo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[cargo_sobreposto_servidor]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cargo_sobreposto_servidor](
	[cd_cargo_sobreposto_servidor] [int] NOT NULL,
	[cd_cargo_base_servidor] [int] NOT NULL,
	[cd_cargo] [int] NOT NULL,
	[cd_unidade_local_servico] [char](6) NULL,
	[qt_carga_horaria_utilizada] [decimal](4, 2) NULL,
	[dt_nomeacao_cargo_sobreposto] [datetime] NOT NULL,
	[nr_ato_cargo_sobreposto] [int] NOT NULL,
	[an_ato_cargo_sobreposto] [decimal](4, 0) NOT NULL,
	[dt_publicacao_doc_cargo_sobreposto] [datetime] NOT NULL,
	[dt_fim_cargo_sobreposto] [datetime] NULL,
	[nr_ato_exoneracao_cargo_sobreposto] [int] NULL,
	[an_ato_exoneracao_cargo_sobreposto] [decimal](4, 0) NULL,
	[dt_publicacao_doc_exoneracao_sobreposto] [datetime] NULL,
	[dt_cancelamento] [datetime] NULL,
	[cd_cargo_sobreposto_sobreposto] [int] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[cd_tipo_ato_nomeacao_cargo_sobreposto] [int] NULL,
	[cd_tipo_ato_exoneracao_cargo_sobreposto] [int] NULL,
 CONSTRAINT [PK_cargo_sobreposto_servidor] PRIMARY KEY CLUSTERED 
(
	[cd_cargo_sobreposto_servidor] ASC,
	[cd_cargo_base_servidor] ASC,
	[cd_cargo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ciclo_ensino]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ciclo_ensino](
	[cd_modalidade_ensino] [int] NOT NULL,
	[cd_etapa_ensino] [int] NOT NULL,
	[cd_ciclo_ensino] [int] NOT NULL,
	[dc_ciclo_ensino] [varchar](20) NOT NULL,
	[nr_ordem_ciclo] [smallint] NOT NULL,
	[dt_cancelamento] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
 CONSTRAINT [PK_ciclo_ensino] PRIMARY KEY CLUSTERED 
(
	[cd_ciclo_ensino] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[componente_curricular]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[componente_curricular](
	[cd_componente_curricular] [int] NOT NULL,
	[dc_componente_curricular] [varchar](70) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[in_polivalencia] [char](1) NOT NULL,
	[in_programa_disciplina] [char](1) NOT NULL,
	[dt_cancelamento] [datetime] NULL,
	[cd_disciplina_mec] [int] NULL,
	[cd_componente_curricular_principal] [int] NOT NULL,
	[cd_componente_equivalente_quadro] [int] NULL,
	[in_sp_integral] [char](1) NOT NULL,
 CONSTRAINT [PK_componente_curricular] PRIMARY KEY CLUSTERED 
(
	[cd_componente_curricular] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[contrato_externo]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[contrato_externo](
	[cd_contrato_externo] [bigint] NOT NULL,
	[cd_pessoa] [bigint] NOT NULL,
	[cd_tipo_funcao_funcionario_externo] [int] NOT NULL,
	[cd_motivo_desligamento_externo] [bigint] NULL,
	[cd_unidade_educacao] [varchar](6) NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[vl_salario_base] [decimal](10, 2) NULL,
	[vl_salario_base_fgts] [decimal](10, 2) NULL,
	[vl_inss] [decimal](10, 2) NULL,
	[vl_vale_transporte] [decimal](10, 2) NULL,
	[dt_cancelamento] [datetime] NULL,
	[cd_operador] [varchar](8) NULL,
	[dt_atualizacao_tabela] [datetime] NULL,
	[cd_instituicao] [varchar](6) NULL,
PRIMARY KEY CLUSTERED 
(
	[cd_contrato_externo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[dispositivo_comunicacao_unidade]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dispositivo_comunicacao_unidade](
	[cd_dispositivo_comunicacao_unidade] [int] NOT NULL,
	[cd_unidade_educacao] [char](6) NULL,
	[cd_local_dispositivo_comunicacao] [int] NOT NULL,
	[tp_dispositivo_comunicacao] [int] NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[cd_ddd] [char](4) NULL,
	[dc_dispositivo] [varchar](50) NOT NULL,
	[cd_ramal] [char](4) NULL,
	[nm_contato] [varchar](20) NULL,
	[dt_fim] [smalldatetime] NULL,
	[cd_operador] [char](8) NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
 CONSTRAINT [PK_dispositivo_comunicacao_unidade] PRIMARY KEY CLUSTERED 
(
	[cd_dispositivo_comunicacao_unidade] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[duracao_tipo_turno]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[duracao_tipo_turno](
	[cd_tipo_turno] [int] NOT NULL,
	[cd_duracao] [int] NOT NULL,
	[qt_hora_duracao] [smallint] NOT NULL,
	[qt_minuto_duracao] [smallint] NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
 CONSTRAINT [PK_duracao_tipo_turno] PRIMARY KEY CLUSTERED 
(
	[cd_tipo_turno] ASC,
	[cd_duracao] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[duracao_turno_escola]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[duracao_turno_escola](
	[cd_escola] [char](6) NOT NULL,
	[cd_tipo_turno] [int] NOT NULL,
	[cd_duracao] [int] NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[cd_escola] ASC,
	[cd_tipo_turno] ASC,
	[cd_duracao] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[escola]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[escola](
	[cd_escola] [char](6) NOT NULL,
	[tp_voltagem] [int] NULL,
	[tp_local_reservatorio_agua] [int] NULL,
	[tp_muro_fecho] [int] NULL,
	[tp_abastecimento_gas] [int] NULL,
	[cd_cie_unidade] [int] NULL,
	[cd_cnpj_entidade_executora] [bigint] NULL,
	[in_oferece_merenda] [char](1) NULL,
	[cd_cnpj_unidade_privada] [bigint] NULL,
	[cd_cnas_municipal] [bigint] NULL,
	[cd_cnas_estadual] [bigint] NULL,
	[cd_cnas_federal] [bigint] NULL,
	[cd_ceff_unidade] [bigint] NULL,
	[in_localizacao_area_quilombo] [char](1) NULL,
	[in_existencia_material_didatico_quilombola] [char](1) NULL,
	[in_localizacao_area_assentamento] [char](1) NULL,
	[in_localizacao_area_terra_indigena] [char](1) NULL,
	[in_educacao_indigena] [char](1) NULL,
	[in_utilizacao_material_didatico_indigena] [char](1) NULL,
	[in_existencia_rede_local] [char](1) NULL,
	[tp_educacao_profissional] [char](1) NULL,
	[qt_area_edificada] [int] NULL,
	[qt_area_ocupada] [int] NULL,
	[qt_area_livre] [int] NULL,
	[qt_area_total] [int] NULL,
	[qt_pavimento] [int] NULL,
	[an_construcao] [smallint] NULL,
	[in_utilizacao_agua_filtrada] [char](1) NULL,
	[tp_lingua_indigena] [int] NULL,
	[tp_periodo_limpeza_caixa_agua] [int] NULL,
	[tp_proprietario] [int] NULL,
	[tp_forma_ocupacao_predio] [int] NULL,
	[tp_dispositivo_lixo] [int] NULL,
	[tp_escola] [int] NULL,
	[cd_operador] [char](8) NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[tp_dependencia_administrativa] [int] NULL,
	[in_rua_pavimentada] [char](1) NULL,
	[in_rua_iluminada] [char](1) NULL,
	[tp_grade] [char](1) NULL,
	[in_merenda_terceirizada] [char](1) NULL,
	[in_limpeza_terceirizada] [char](1) NULL,
	[in_programa_especial] [char](1) NULL,
 CONSTRAINT [PK_escola] PRIMARY KEY CLUSTERED 
(
	[cd_escola] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[escola_grade]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[escola_grade](
	[cd_escola_grade] [int] NOT NULL,
	[dt_inicio_validade] [datetime] NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[cd_grade] [int] NOT NULL,
	[cd_escola] [char](6) NOT NULL,
	[dt_fim_validade] [datetime] NULL,
 CONSTRAINT [PK_escola_grade] PRIMARY KEY CLUSTERED 
(
	[cd_escola_grade] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[etapa_ensino]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[etapa_ensino](
	[cd_modalidade_ensino] [int] NOT NULL,
	[cd_etapa_ensino] [int] NOT NULL,
	[dc_etapa_ensino] [varchar](60) NOT NULL,
	[nr_ordem_etapa] [smallint] NOT NULL,
	[sg_etapa] [varchar](10) NOT NULL,
	[in_ciclo] [char](1) NOT NULL,
	[in_seriada] [char](1) NOT NULL,
	[dt_cancelamento] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[tp_nivel_exibicao_pre_matricula] [int] NOT NULL,
 CONSTRAINT [PK_etapa_ensino] PRIMARY KEY CLUSTERED 
(
	[cd_modalidade_ensino] ASC,
	[cd_etapa_ensino] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[funcao_atividade_cargo_servidor]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[funcao_atividade_cargo_servidor](
	[cd_funcao_cargo_servidor] [int] NOT NULL,
	[cd_cargo_base_servidor] [int] NOT NULL,
	[cd_tipo_funcao] [int] NOT NULL,
	[cd_unidade_local_servico] [char](6) NULL,
	[qt_carga_horaria] [decimal](4, 2) NOT NULL,
	[dt_designacao] [datetime] NOT NULL,
	[nr_ato_funcao_atividade] [int] NOT NULL,
	[an_ato_funcao_atividade] [decimal](4, 0) NOT NULL,
	[dt_publicacao_doc] [datetime] NOT NULL,
	[dt_fim_funcao_atividade] [datetime] NULL,
	[nr_ato_cessacao] [int] NULL,
	[an_ato_cessacao] [decimal](4, 0) NULL,
	[dt_publicacao_doc_cessacao] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[dt_cancelamento] [datetime] NULL,
	[cd_tipo_ato_designacao] [int] NULL,
	[dt_convocacao_designacao] [datetime] NULL,
	[dt_publicacao_convocacao_designacao] [datetime] NULL,
	[nr_ato_convocacao_designacao] [int] NULL,
	[an_convocacao_designacao] [decimal](4, 0) NULL,
	[dt_fim_convocacao] [datetime] NULL,
	[dt_publicacao_fim_convocacao] [datetime] NULL,
	[nr_ato_fim_convocacao] [int] NULL,
	[an_fim_convocacao] [decimal](4, 0) NULL,
	[cd_tipo_ato_cessacao] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[cd_funcao_cargo_servidor] ASC,
	[cd_cargo_base_servidor] ASC,
	[cd_tipo_funcao] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[funcao_externo]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[funcao_externo](
	[cd_funcao_externo] [int] NOT NULL,
	[dc_funcao_externo] [varchar](70) NOT NULL,
	[qt_carga_horaria] [smallint] NOT NULL,
	[in_permissao_atribuicao_aula] [char](1) NOT NULL,
	[dt_inicio] [datetime] NULL,
	[dt_cancelamento] [datetime] NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[in_curso_superior] [char](1) NULL,
 CONSTRAINT [PK_funcao_externo] PRIMARY KEY CLUSTERED 
(
	[cd_funcao_externo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[funcao_funcionario_externo]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[funcao_funcionario_externo](
	[cd_tipo_funcao_funcionario_externo] [int] NOT NULL,
	[cd_funcao_externo] [int] NOT NULL,
	[cd_tipo_funcionario_externo] [int] NOT NULL,
	[dt_inicio] [datetime] NULL,
	[dt_cancelamento] [datetime] NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
 CONSTRAINT [PK_funcao_funcionario_externo] PRIMARY KEY CLUSTERED 
(
	[cd_tipo_funcao_funcionario_externo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[grade]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[grade](
	[cd_grade] [int] NOT NULL,
	[cd_tipo_grade] [int] NOT NULL,
	[cd_tipo_turno] [int] NOT NULL,
	[cd_tipo_turma] [int] NOT NULL,
	[dt_inicio_validade] [datetime] NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[cd_duracao] [int] NOT NULL,
	[cd_serie_ensino] [int] NULL,
	[cd_tipo_habilitacao_profissional] [int] NULL,
	[cd_tipo_projeto] [int] NULL,
	[cd_tipo_programa] [int] NULL,
	[dt_fim_validade] [datetime] NULL,
	[dt_diario_oficial] [datetime] NULL,
	[dc_grade] [char](70) NOT NULL,
 CONSTRAINT [PK_grade] PRIMARY KEY CLUSTERED 
(
	[cd_grade] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[grade_componente_curricular]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[grade_componente_curricular](
	[cd_grade] [int] NOT NULL,
	[cd_componente_curricular] [int] NOT NULL,
	[qt_aula_semanal] [decimal](3, 0) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[nr_ordem] [smallint] NOT NULL,
	[tp_agrupamento] [char](1) NOT NULL,
	[qt_hora_professor] [decimal](3, 0) NULL,
 CONSTRAINT [PK_grade_componente_curricular] PRIMARY KEY CLUSTERED 
(
	[cd_grade] ASC,
	[cd_componente_curricular] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[historico_matricula_turma_escola]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[historico_matricula_turma_escola](
	[cd_matricula] [int] NOT NULL,
	[cd_turma_escola] [int] NOT NULL,
	[dt_situacao_aluno] [datetime] NOT NULL,
	[cd_situacao_aluno] [numeric](18, 0) NULL,
	[tp_origem_matricula] [char](1) NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_atlz_tab] [datetime] NOT NULL,
	[nr_chamada_aluno] [varchar](5) NULL,
	[cd_turma_escola_reserva1] [int] NULL,
	[cd_turma_escola_reserva2] [int] NULL,
	[nro_chamada_prodesp] [char](3) NULL,
	[in_utilizacao_estorno] [char](1) NULL,
	[in_solicitacao_transferencia] [char](1) NULL,
 CONSTRAINT [PK_historico_matricula_turma_escola] PRIMARY KEY CLUSTERED 
(
	[cd_matricula] ASC,
	[cd_turma_escola] ASC,
	[dt_situacao_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[historico_unidade]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[historico_unidade](
	[cd_historico_unidade] [int] NOT NULL,
	[dt_inicio_ocorrencia_unidade] [datetime] NOT NULL,
	[tp_ocorrencia_historica] [int] NOT NULL,
	[cd_ato] [varchar](15) NULL,
	[dc_ato] [varchar](100) NULL,
	[nr_ato] [varchar](20) NULL,
	[dt_publicacao_dom] [smalldatetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[cd_unidade_educacao] [char](6) NULL,
	[tp_atualizacao_registro] [char](1) NULL,
 CONSTRAINT [PK_historico_escola] PRIMARY KEY CLUSTERED 
(
	[cd_historico_unidade] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[laudo_medico]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[laudo_medico](
	[cd_laudo_medico] [int] NOT NULL,
	[cd_tipo_laudo] [char](1) NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_publicacao_doc_laudo_medico] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_cargo_base_servidor] [int] NOT NULL,
	[dt_cancelamento] [datetime] NULL,
	[dt_prevista_termino_laudo_medico] [datetime] NULL,
	[dt_publicacao_doc_cessacao_laudo] [datetime] NULL,
 CONSTRAINT [PK_laudo_medico] PRIMARY KEY CLUSTERED 
(
	[cd_laudo_medico] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lotacao_servidor]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lotacao_servidor](
	[cd_lotacao_servidor] [int] NOT NULL,
	[cd_cargo_base_servidor] [int] NOT NULL,
	[cd_unidade_educacao] [char](6) NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[cd_motivo_desligamento] [int] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[nr_ato_lotacao] [int] NULL,
	[an_ato_lotacao] [decimal](4, 0) NULL,
	[dt_publicacao_doc_lotacao] [datetime] NULL,
	[cd_tipo_ato] [int] NULL,
	[tp_lotacao_servidor] [char](1) NULL,
	[dt_cancelamento] [datetime] NULL,
 CONSTRAINT [PK_local_servico_servidor] PRIMARY KEY CLUSTERED 
(
	[cd_lotacao_servidor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[matricula_turma_escola]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[matricula_turma_escola](
	[cd_matricula] [int] NOT NULL,
	[cd_turma_escola] [int] NOT NULL,
	[dt_situacao_aluno] [datetime] NOT NULL,
	[cd_situacao_aluno] [numeric](18, 0) NULL,
	[tp_origem_matricula] [char](1) NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_atlz_tab] [datetime] NOT NULL,
	[nr_chamada_aluno] [varchar](5) NULL,
	[cd_turma_escola_reserva1] [int] NULL,
	[cd_turma_escola_reserva2] [int] NULL,
	[nro_chamada_prodesp] [char](3) NULL,
	[in_solicitacao_transferencia] [char](1) NULL,
 CONSTRAINT [PK_matricula_turma_escola] PRIMARY KEY CLUSTERED 
(
	[cd_matricula] ASC,
	[cd_turma_escola] ASC,
	[dt_situacao_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[municipio]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[municipio](
	[cd_municipio] [int] NOT NULL,
	[nm_municipio] [varchar](100) NULL,
	[cd_municipio_mec] [bigint] NOT NULL,
	[an_inclusao_municipio] [smallint] NOT NULL,
	[sg_uf] [char](2) NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[dt_cancelamento] [datetime] NULL,
	[cd_cep_inicio] [int] NULL,
	[cd_cep_fim] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[cd_municipio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[necessidade_especial_aluno]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[necessidade_especial_aluno](
	[cd_identificador_necessidade_especial_aluno] [int] NOT NULL,
	[cd_aluno] [int] NULL,
	[tp_necessidade_especial] [int] NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
 CONSTRAINT [PK_necessidade_especial_aluno] PRIMARY KEY CLUSTERED 
(
	[cd_identificador_necessidade_especial_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[orgao_emissor]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[orgao_emissor](
	[cd_orgao_emissor] [int] NOT NULL,
	[nm_orgao_emissor] [varchar](60) NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[dt_cancelamento] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[cd_orgao_emissor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[pessoa]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[pessoa](
	[cd_pessoa] [int] NOT NULL,
	[nm_pessoa] [varchar](70) NULL,
	[nm_pai_pessoa] [varchar](70) NULL,
	[cd_sexo_pessoa] [char](1) NULL,
	[dt_nascimento_pessoa] [datetime] NULL,
	[nm_mae_pessoa] [varchar](70) NULL,
	[dt_entrada_pais] [datetime] NULL,
	[cd_identificacao_social] [varchar](20) NULL,
	[nr_rg_pessoa] [varchar](15) NULL,
	[cd_complemento_rg] [varchar](4) NULL,
	[cd_orgao_emissor_rg] [int] NULL,
	[dt_emissao_rg] [datetime] NULL,
	[sg_uf_rg] [char](2) NULL,
	[cd_cpf_pessoa] [varchar](11) NULL,
	[nr_titulo_eleitor_pessoa] [varchar](15) NULL,
	[cd_zona_titulo_eleitor_pessoa] [char](3) NULL,
	[cd_secao_titulo_eleitor_pessoa] [char](4) NULL,
	[tp_certidao] [char](1) NULL,
	[nr_certidao] [varchar](8) NULL,
	[nr_folha_certidao] [char](4) NULL,
	[nr_livro_certidao] [varchar](8) NULL,
	[dt_emissao_certidao] [datetime] NULL,
	[cd_distrito_certidao] [int] NULL,
	[cd_municipio_certidao] [int] NULL,
	[cd_municipio_nascimento] [int] NULL,
	[cd_pais_mec] [int] NULL,
	[cd_pis_pasep] [varchar](11) NULL,
	[nr_carteira_trabalho] [char](8) NULL,
	[nr_serie_carteira] [char](3) NULL,
	[tp_raca_cor] [int] NOT NULL,
	[cd_nacionalidade_pessoa] [char](1) NULL,
	[cd_rne_passaporte] [varchar](15) NULL,
	[in_documento_pendente] [char](1) NULL,
	[tp_necessidade_especial] [int] NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[cd_inep_pessoa] [decimal](12, 0) NULL,
	[cd_matricula_certidao_pessoa] [char](32) NULL,
	[nm_social] [varchar](70) NULL,
 CONSTRAINT [PK_pessoa] PRIMARY KEY CLUSTERED 
(
	[cd_pessoa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[recurso_aluno]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[recurso_aluno](
	[cd_recurso_aluno] [int] NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[cd_aluno] [int] NOT NULL,
	[cd_tipo_recurso] [smallint] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[responsavel_aluno]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[responsavel_aluno](
	[cd_identificador_responsavel] [int] NOT NULL,
	[cd_aluno] [int] NULL,
	[tp_pessoa_responsavel] [decimal](1, 0) NULL,
	[nm_responsavel] [char](70) NULL,
	[ci_endereco] [bigint] NULL,
	[nr_rg_responsavel] [char](15) NULL,
	[cd_digito_rg_responsavel] [char](4) NULL,
	[sg_uf_rg_responsavel] [char](2) NULL,
	[cd_cpf_responsavel] [decimal](11, 0) NULL,
	[cd_ddd_celular_responsavel] [char](4) NULL,
	[nr_celular_responsavel] [char](9) NULL,
	[cd_ddd_telefone_fixo_responsavel] [char](4) NULL,
	[nr_telefone_fixo_responsavel] [char](9) NULL,
	[cd_situacao_documento_responsavel] [char](1) NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_ddd_telefone_comercial_responsavel] [char](4) NULL,
	[nr_telefone_comercial_responsavel] [char](9) NULL,
	[nr_ramal_telefone_comercial_responsavel] [char](4) NULL,
	[in_cpf_responsavel_confere] [char](1) NULL,
	[in_autoriza_envio_sms] [char](1) NULL,
	[email_responsavel] [varchar](50) NULL,
	[cd_tipo_turno_celular] [int] NULL,
	[cd_tipo_turno_comercial] [int] NULL,
	[cd_tipo_turno_fixo] [int] NULL,
	[cd_endereco_comercial] [bigint] NULL,
	[nm_mae_responsavel] [varchar](70) NULL,
	[dt_nascimento_mae_responsavel] [datetime] NULL,
 CONSTRAINT [PK_responsavel_aluno] PRIMARY KEY CLUSTERED 
(
	[cd_identificador_responsavel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[serie_ensino]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[serie_ensino](
	[cd_serie_ensino] [int] NOT NULL,
	[cd_modalidade_ensino] [int] NOT NULL,
	[cd_etapa_ensino] [int] NOT NULL,
	[cd_ciclo_ensino] [int] NOT NULL,
	[dc_serie_ensino] [varchar](40) NOT NULL,
	[nr_ordem_serie] [smallint] NOT NULL,
	[cd_tipo_periodicidade] [int] NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[in_quadro_vaga] [char](1) NOT NULL,
	[tp_censo_escolar] [char](1) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[sg_serie_ensino] [varchar](18) NOT NULL,
	[tp_compatibilizacao_escola] [char](1) NOT NULL,
	[qt_idade_minima] [int] NOT NULL,
	[qt_idade_maxima] [int] NOT NULL,
	[sg_resumida_serie] [char](1) NOT NULL,
	[in_permite_rematricula_proxima_serie] [varchar](1) NULL,
	[in_uniforme_escolar] [char](1) NULL,
	[in_demanda_relatorio] [char](1) NULL,
	[in_atribuicao_aula] [char](1) NOT NULL,
	[in_leve_leite] [char](1) NULL,
	[in_aleitamento] [char](1) NULL,
	[qt_distancia_encaminhamento] [decimal](10, 0) NULL,
	[in_escola_particular] [char](1) NULL,
 CONSTRAINT [PK_serie_ensino] PRIMARY KEY CLUSTERED 
(
	[cd_serie_ensino] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[serie_escola]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[serie_escola](
	[cd_escola] [char](6) NOT NULL,
	[cd_serie_ensino] [int] NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_grade_distancia] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[cd_escola] ASC,
	[cd_serie_ensino] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[serie_turma_escola]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[serie_turma_escola](
	[cd_turma_escola] [int] NOT NULL,
	[cd_escola] [char](6) NOT NULL,
	[cd_serie_ensino] [int] NOT NULL,
	[cd_tipo_habilitacao_profissional] [int] NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[cd_turma_escola] ASC,
	[cd_escola] ASC,
	[cd_serie_ensino] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[serie_turma_grade]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[serie_turma_grade](
	[cd_serie_grade] [int] NOT NULL,
	[cd_turma_escola] [int] NULL,
	[cd_escola] [char](6) NULL,
	[cd_serie_ensino] [int] NULL,
	[cd_escola_grade] [int] NULL,
	[dt_inicio] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_atlz_tab] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
 CONSTRAINT [PK_serie_turma_grade] PRIMARY KEY CLUSTERED 
(
	[cd_serie_grade] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[sub_prefeitura]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[sub_prefeitura](
	[cd_sub_prefeitura] [int] NOT NULL,
	[sg_sub_prefeitura] [char](3) NULL,
	[dc_sub_prefeitura] [varchar](35) NULL,
	[cd_operador] [char](8) NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[dt_cancelamento] [smalldatetime] NULL,
 CONSTRAINT [PK_sub_prefeitura] PRIMARY KEY CLUSTERED 
(
	[cd_sub_prefeitura] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[territorio_saber]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[territorio_saber](
	[cd_territorio_saber] [int] NOT NULL,
	[dc_territorio_saber] [varchar](70) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_cancelamento] [datetime] NULL,
 CONSTRAINT [PK_territorio_saber] PRIMARY KEY CLUSTERED 
(
	[cd_territorio_saber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_dispositivo_comunicacao]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_dispositivo_comunicacao](
	[tp_dispositivo_comunicacao] [int] NOT NULL,
	[dc_tipo_dispositivo_comunicacao] [varchar](25) NULL,
	[cd_operador] [char](8) NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[dt_cancelamento] [smalldatetime] NULL,
 CONSTRAINT [PK_tipo_dispositivo_comunicacao] PRIMARY KEY CLUSTERED 
(
	[tp_dispositivo_comunicacao] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_escola]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_escola](
	[tp_escola] [int] NOT NULL,
	[tp_dependencia_administrativa] [int] NOT NULL,
	[dc_tipo_escola] [varchar](70) NULL,
	[sg_tp_escola] [char](12) NULL,
	[dt_cancelamento] [smalldatetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[in_exibicao_portal] [char](1) NULL,
	[dc_exibicao_portal] [varchar](80) NULL,
	[in_uniforme_escolar] [char](1) NOT NULL,
 CONSTRAINT [PK_tipo_escola] PRIMARY KEY CLUSTERED 
(
	[tp_escola] ASC,
	[tp_dependencia_administrativa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_experiencia_pedagogica]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_experiencia_pedagogica](
	[cd_experiencia_pedagogica] [int] NOT NULL,
	[dc_experiencia_pedagogica] [varchar](70) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_cancelamento] [datetime] NULL,
 CONSTRAINT [PK_tipo_experiencia_pedagogica] PRIMARY KEY CLUSTERED 
(
	[cd_experiencia_pedagogica] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_logradouro]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_logradouro](
	[tp_logradouro] [int] NOT NULL,
	[dc_tp_logradouro] [varchar](20) NULL,
	[nr_ordem_logradouro] [decimal](18, 0) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[dt_cancelamento] [smalldatetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_necessidade_especial]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_necessidade_especial](
	[tp_necessidade_especial] [int] NOT NULL,
	[dc_necessidade_especial] [varchar](30) NULL,
	[dt_cancelamento] [datetime] NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[tp_necessidade_especial_estado] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_periodicidade]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_periodicidade](
	[cd_tipo_periodicidade] [int] NOT NULL,
	[dc_tipo_periodicidade] [varchar](40) NOT NULL,
	[dt_cancelamento] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[in_calendario] [char](1) NOT NULL,
	[qt_dia_padrao] [smallint] NULL,
	[qt_dia_tolerancia] [smallint] NULL,
 CONSTRAINT [PK__tipo_periodicida__1CFE9F3D] PRIMARY KEY CLUSTERED 
(
	[cd_tipo_periodicidade] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_programa]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_programa](
	[cd_tipo_programa] [int] NOT NULL,
	[cd_modalidade_ensino] [int] NOT NULL,
	[dc_tipo_programa] [varchar](40) NOT NULL,
	[in_calendario] [char](1) NOT NULL,
	[dt_cancelamento] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[sg_tipo_programa] [varchar](20) NOT NULL,
	[in_obrigatoriedade_matricula_seriada] [char](1) NOT NULL,
	[in_compatibilizacao_escola] [char](1) NOT NULL,
	[in_obrigatoriedade_autorizacao_especial] [char](1) NOT NULL,
	[in_censo_escolar] [char](1) NOT NULL,
	[sg_resumida_tipo_programa] [char](1) NOT NULL,
	[cd_tipo_atendimento] [int] NULL,
	[tp_ambiente] [int] NULL,
	[in_atribuicao_aula] [char](1) NULL,
	[cd_tipo_educacao_especializada] [int] NULL,
	[in_parceira_educacao_especial] [char](1) NOT NULL,
	[in_sobreposicao_horario] [char](1) NOT NULL,
 CONSTRAINT [PK__tipo_programa] PRIMARY KEY CLUSTERED 
(
	[cd_tipo_programa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_recurso_aluno]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_recurso_aluno](
	[cd_tipo_recurso] [smallint] NOT NULL,
	[dc_tipo_recurso] [varchar](40) NOT NULL,
	[dt_cancelamento] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_recurso_aluno_incompativel]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_recurso_aluno_incompativel](
	[cd_tipo_recurso] [smallint] NOT NULL,
	[cd_tipo_recurso_incompativel] [smallint] NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_turma]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_turma](
	[cd_tipo_turma] [int] NOT NULL,
	[dc_tipo_turma] [varchar](20) NOT NULL,
	[dt_cancelamento] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[cd_subtipo_turma] [tinyint] NOT NULL,
	[cd_tipo_tratamento_grade] [char](1) NOT NULL,
 CONSTRAINT [PK__tipo_turma__22B77893] PRIMARY KEY CLUSTERED 
(
	[cd_tipo_turma] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_turno]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_turno](
	[cd_tipo_turno] [int] NOT NULL,
	[dc_tipo_turno] [varchar](20) NOT NULL,
	[sg_tipo_turno] [varchar](10) NOT NULL,
	[ho_inicio_padrao] [char](4) NOT NULL,
	[ho_inicio_tolerancia] [char](4) NOT NULL,
	[qt_hora_acrescimo_tolerancia] [char](4) NOT NULL,
	[dt_cancelamento] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[dc_exibicao_portal] [varchar](30) NULL,
	[ho_maxima_termino_turno] [char](4) NOT NULL,
 CONSTRAINT [PK_tipo_turno] PRIMARY KEY CLUSTERED 
(
	[cd_tipo_turno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tipo_unidade_educacao]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tipo_unidade_educacao](
	[tp_unidade_educacao] [int] NOT NULL,
	[dc_tipo_unidade_educacao] [varchar](25) NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NULL,
	[dt_cancelamento] [smalldatetime] NULL,
 CONSTRAINT [PK_tipo_unidade_educacao] PRIMARY KEY CLUSTERED 
(
	[tp_unidade_educacao] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[turma_escola]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[turma_escola](
	[cd_turma_escola] [int] NOT NULL,
	[cd_tipo_periodicidade] [int] NOT NULL,
	[an_letivo] [smallint] NOT NULL,
	[cd_escola] [char](6) NOT NULL,
	[dc_turma_escola] [varchar](15) NOT NULL,
	[cd_ambiente_escola] [int] NOT NULL,
	[cd_tipo_atendimento] [int] NOT NULL,
	[cd_tipo_turma] [int] NOT NULL,
	[cd_tipo_programa] [int] NULL,
	[qt_vaga_oferecida] [tinyint] NOT NULL,
	[st_turma_escola] [char](1) NOT NULL,
	[ho_entrada] [char](4) NOT NULL,
	[ho_saida] [char](4) NOT NULL,
	[dt_inicio_turma] [datetime] NOT NULL,
	[dt_fim_turma] [datetime] NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
	[cd_operador] [char](8) NOT NULL,
	[cd_tipo_turno] [int] NOT NULL,
	[cd_duracao] [int] NOT NULL,
	[cd_correspondencia_serie] [int] NULL,
	[dt_status_turma_escola] [datetime] NULL,
	[cd_turma_prodesp] [int] NULL,
	[cd_tipo_turno_texto] [char](2) NOT NULL,
 CONSTRAINT [PK_turma_escola] PRIMARY KEY CLUSTERED 
(
	[cd_turma_escola] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[turma_escola_grade_programa]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[turma_escola_grade_programa](
	[cd_turma_escola_grade_programa] [int] NOT NULL,
	[cd_turma_escola] [int] NOT NULL,
	[cd_escola_grade] [int] NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL,
	[cd_operador] [char](8) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
 CONSTRAINT [PK_turma_escola_grade_programa] PRIMARY KEY CLUSTERED 
(
	[cd_turma_escola_grade_programa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[turma_grade_territorio_experiencia]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[turma_grade_territorio_experiencia](
	[cd_serie_grade] [int] NOT NULL,
	[cd_componente_curricular] [int] NOT NULL,
	[cd_territorio_saber] [int] NOT NULL,
	[cd_experiencia_pedagogica] [int] NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[cd_operador] [char](10) NOT NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
 CONSTRAINT [PK_turma_grade_territorio_experiencia] PRIMARY KEY CLUSTERED 
(
	[cd_serie_grade] ASC,
	[cd_componente_curricular] ASC,
	[cd_territorio_saber] ASC,
	[cd_experiencia_pedagogica] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[unidade_administrativa]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[unidade_administrativa](
	[cd_unidade_administrativa] [char](6) NOT NULL,
	[tp_unidade_administrativa] [int] NOT NULL,
	[cd_operador] [char](8) NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[cd_unidade_administrativa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[unidadeTeste]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[unidadeTeste](
	[cd_unidade_administrativa] [char](6) NOT NULL,
	[tp_unidade_administrativa] [int] NOT NULL,
	[cd_operador] [char](8) NULL,
	[dt_atualizacao_tabela] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[v_aluno_cotic]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[v_aluno_cotic](
	[cd_aluno] [int] NOT NULL,
	[nm_aluno] [varchar](70) NOT NULL,
	[dt_nascimento_aluno] [datetime] NOT NULL,
	[cd_sexo_aluno] [char](1) NOT NULL,
	[nm_mae_aluno] [varchar](70) NOT NULL,
	[tp_raca_cor] [int] NULL,
	[nm_pai_aluno] [varchar](70) NULL,
	[cd_inep_aluno] [numeric](12, 0) NULL,
	[cd_aluno_principal] [int] NULL,
	[nm_social_aluno] [varchar](70) NULL,
	[cd_tipo_sigilo] [int] NULL,
	[cd_sexo_mae] [char](1) NULL,
	[cd_sexo_pai] [char](1) NULL,
 CONSTRAINT [PK_v_aluno_cotic] PRIMARY KEY CLUSTERED 
(
	[cd_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[v_cadastro_unidade_educacao]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[v_cadastro_unidade_educacao](
	[cd_unidade_educacao] [char](6) NOT NULL,
	[nm_unidade_educacao] [varchar](60) NULL,
	[nm_exibicao_unidade] [varchar](60) NULL,
	[cd_autorizacao] [int] NULL,
	[tp_unidade_educacao] [int] NOT NULL,
	[cd_cie_unidade_educacao] [int] NULL,
	[cd_unidade_administrativa_referencia] [char](6) NULL,
	[cd_endereco_grh] [varchar](15) NULL,
	[ci_endereco] [bigint] NULL,
	[qt_funcionario] [int] NULL,
	[tp_situacao_unidade] [int] NULL,
	[nr_rgi_sabesp] [varchar](12) NULL,
	[cd_unidade_educacao_prodesp] [varchar](8) NULL,
	[cd_unidade_administrativa_portal] [char](6) NULL,
	[tp_logradouro] [int] NOT NULL,
	[dc_ponto_referencia] [varchar](60) NULL,
	[sg_titulo_logradouro] [varchar](20) NULL,
	[nm_logradouro] [varchar](60) NULL,
	[cd_nr_endereco] [char](6) NOT NULL,
	[dc_complemento_endereco] [varchar](30) NULL,
	[nm_bairro] [varchar](40) NULL,
	[cd_cep] [int] NULL,
	[cd_logradouro] [int] NULL,
	[cd_micro_regiao] [int] NULL,
	[cd_sql_endereco] [bigint] NULL,
	[tp_localizacao_endereco] [char](1) NULL,
	[dt_atualizacao_endereco] [datetime] NOT NULL,
	[cd_caixa_postal] [varchar](14) NULL,
	[cd_distrito_mec] [int] NULL,
	[cd_municipio] [int] NULL,
	[cd_sub_prefeitura] [int] NULL,
	[cd_coordenada_geo_x] [numeric](9, 6) NULL,
	[cd_coordenada_geo_y] [numeric](9, 6) NULL,
PRIMARY KEY CLUSTERED 
(
	[cd_unidade_educacao] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[v_cargo_base_cotic]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[v_cargo_base_cotic](
	[cd_cargo_base_servidor] [int] NOT NULL,
	[cd_servidor] [int] NOT NULL,
	[cd_vinculo_sigpec] [int] NULL,
	[cd_cargo] [int] NOT NULL,
	[cd_situacao_funcional] [int] NOT NULL,
	[dt_nomeacao] [datetime] NULL,
	[dt_posse] [datetime] NULL,
	[dt_inicio_exercicio] [datetime] NULL,
	[dt_fim_nomeacao] [datetime] NULL,
	[dt_cancelamento] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[cd_servidor] ASC,
	[cd_cargo_base_servidor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[v_historico_matricula_cotic]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[v_historico_matricula_cotic](
	[cd_matricula] [int] NOT NULL,
	[dt_status_matricula] [datetime] NOT NULL,
	[st_matricula] [char](1) NOT NULL,
	[tp_matricula] [char](1) NOT NULL,
	[cd_rendimento_parecer_conclusivo] [char](1) NULL,
	[an_letivo] [smallint] NULL,
	[cd_escola] [char](6) NULL,
	[cd_aluno] [int] NULL,
	[cd_serie_ensino] [int] NULL,
	[cd_tipo_habilitacao_profissional] [int] NULL,
	[cd_tipo_programa] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[v_matricula_cotic]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[v_matricula_cotic](
	[cd_matricula] [int] NOT NULL,
	[st_matricula] [char](1) NOT NULL,
	[dt_status_matricula] [datetime] NOT NULL,
	[tp_matricula] [char](1) NOT NULL,
	[cd_rendimento_parecer_conclusivo] [char](1) NULL,
	[an_letivo] [smallint] NULL,
	[cd_escola] [char](6) NULL,
	[cd_aluno] [int] NULL,
	[cd_serie_ensino] [int] NULL,
	[cd_tipo_habilitacao_profissional] [int] NULL,
	[cd_tipo_programa] [int] NULL,
 CONSTRAINT [PK_v_matricula_cotic] PRIMARY KEY CLUSTERED 
(
	[cd_matricula] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[v_servidor_cotic]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[v_servidor_cotic](
	[cd_servidor] [int] NOT NULL,
	[cd_registro_funcional] [char](7) NULL,
	[nm_pessoa] [varchar](70) NULL,
	[cd_sexo_pessoa] [char](1) NULL,
	[dt_nascimento_pessoa] [datetime] NULL,
	[cd_cpf_pessoa] [varchar](11) NULL,
	[nm_social] [varchar](70) NULL,
	[cd_inep_pessoa] [numeric](12, 0) NULL,
PRIMARY KEY CLUSTERED 
(
	[cd_servidor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[v_servidor_email_cotic]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[v_servidor_email_cotic](
	[cd_servidor] [int] NOT NULL,
	[dc_dispositivo] [varchar](50) NOT NULL,
	[dt_inicio] [datetime] NOT NULL,
	[dt_fim] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[v_servidor_mstech_ativos]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[v_servidor_mstech_ativos](
	[cd_registro_funcional] [char](7) NULL,
	[nm_pessoa] [varchar](70) NULL,
	[dt_nascimento_pessoa] [datetime] NULL,
	[cd_sexo_pessoa] [char](1) NULL,
	[nr_rg_pessoa] [varchar](15) NULL,
	[cd_complemento_rg] [varchar](4) NULL,
	[codigo_orgao_emissor_rg] [int] NULL,
	[dt_emissao_rg] [datetime] NULL,
	[sg_uf_rg] [char](2) NULL,
	[cd_cpf_pessoa] [varchar](11) NULL,
	[situacao] [varchar](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  UserDefinedFunction [dbo].[proc_ObterComponentesCurricularesPorTurma]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[proc_ObterComponentesCurricularesPorTurma]
(	
	@p_cd_turma_escola AS INT
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT iif(pcc.cd_componente_curricular is NOT NULL, pcc.cd_componente_curricular,
                    cc.cd_componente_curricular) as Codigo,
					iif(pcc.dc_componente_curricular is NOT NULL, pcc.dc_componente_curricular,
						cc.dc_componente_curricular) as Descricao,
						serie_ensino.sg_resumida_serie   as AnoTurma
	FROM 
		turma_escola te (NOLOCK)
	INNER JOIN 
		escola esc (NOLOCK) 
		ON te.cd_escola = esc.cd_escola
	INNER JOIN 
		v_cadastro_unidade_educacao (NOLOCK) ue 
		ON ue.cd_unidade_educacao = esc.cd_escola
	INNER JOIN 
		unidade_administrativa dre (NOLOCK) 
		ON dre.tp_unidade_administrativa = 24 AND ue.cd_unidade_administrativa_referencia = dre.cd_unidade_administrativa
	--Serie Ensino
	LEFT JOIN 
		serie_turma_escola (NOLOCK) 
		ON serie_turma_escola.cd_turma_escola = te.cd_turma_escola
	LEFT JOIN 
		serie_turma_grade (NOLOCK) 
		ON serie_turma_grade.cd_turma_escola = serie_turma_escola.cd_turma_escola and serie_turma_grade.dt_fim is NULL
	LEFT JOIN 
		escola_grade (NOLOCK) 
		ON serie_turma_grade.cd_escola_grade = escola_grade.cd_escola_grade
	LEFT JOIN 
		grade (NOLOCK) 
		ON escola_grade.cd_grade = grade.cd_grade
	LEFT JOIN 
		grade_componente_curricular gcc (NOLOCK) 
		ON gcc.cd_grade = grade.cd_grade
	LEFT JOIN 
		componente_curricular cc (NOLOCK) 
		ON cc.cd_componente_curricular = gcc.cd_componente_curricular and cc.dt_cancelamento is NULL
	LEFT JOIN 
		serie_ensino
		ON grade.cd_serie_ensino = serie_ensino.cd_serie_ensino
	-- Programa
	LEFT JOIN 
		tipo_programa tp (NOLOCK) 
		ON te.cd_tipo_programa = tp.cd_tipo_programa
	LEFT JOIN 
		turma_escola_grade_programa tegp (NOLOCK) 
		ON tegp.cd_turma_escola = te.cd_turma_escola
	LEFT JOIN 
		escola_grade teg (NOLOCK) 
		ON teg.cd_escola_grade = tegp.cd_escola_grade
	LEFT JOIN 
		grade pg (NOLOCK) 
		ON pg.cd_grade = teg.cd_grade
	LEFT JOIN 
		grade_componente_curricular pgcc 
		ON pgcc.cd_grade = teg.cd_grade
	LEFT JOIN 
		componente_curricular pcc 
		ON pgcc.cd_componente_curricular = pcc.cd_componente_curricular and pcc.dt_cancelamento is NULL
	WHERE 
		te.cd_turma_escola = @p_cd_turma_escola
		AND te.st_turma_escola in ('O', 'A', 'C')
)
GO
/****** Object:  UserDefinedFunction [dbo].[proc_string_split]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--
-- Function por causa da versao do SQLServer, no 2016 já existe a string_split
--
create function [dbo].[proc_string_split] (@string varchar(max), @separador char(1)) 
RETURNS table as return
    with a as (
        select
            id = 1,
            len_string = len(@string) + 1,
            ini = 1,
            fim = coalesce(nullif(charindex(@separador, @string, 1), 0), len(@string) + 1),
            elemento = ltrim(rtrim(substring(@string, 1, coalesce(nullif(charindex(@separador, @string, 1), 0), len(@string) + 1)-1)))
        union all
        select
            id + 1,
            len(@string) + 1,
            convert(int, fim) + 1,
            coalesce(nullif(charindex(@separador, @string, fim + 1), 0), len_string), 
            ltrim(rtrim(substring(@string, fim + 1, coalesce(nullif(charindex(@separador, @string, fim + 1), 0), len_string)-fim-1)))
        from a where fim < len_string)
    select id, elemento from a;
GO
/****** Object:  View [dbo].[ix_vw_consulta3]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[ix_vw_consulta3] WITH SCHEMABINDING
AS
SELECT
				tur.cd_escola       AS codescola,
				tur.cd_turma_escola AS codturma,
				tur.dt_inicio as dataInicioTurma,
				tur.an_letivo AS anoletivo,
				/*Iif(tur.cd_tipo_turma = 1,
					CASE
						WHEN ee.cd_etapa_ensino IN (@etapasInfantil1,@etapasInfantil2) THEN 'Infantil'
						WHEN ee.cd_etapa_ensino IN (@etapasEja1,@etapasEja2) THEN 'EJA' 
                        WHEN ee.cd_etapa_ensino IN (@etapasFundamental1,@etapasFundamental2,@etapasFundamental3,@etapasFundamental4) THEN 'Fundamental' 
                      WHEN ee.cd_etapa_ensino IN (@etapasMedio1,@etapasMedio2,@etapasMedio3,@etapasMedio4,@etapasMedio5) THEN 'Médio' 
				END, 'Fundamental') AS modalidade,
				Iif(tur.cd_tipo_turma = 1,
					CASE
					WHEN ee.cd_etapa_ensino IN (@etapasEja1,@etapasEja2) THEN
						Iif(Datepart(month, tur.dt_inicio_turma) > 6, 2, 1)
						ELSE 0
				END, 0) AS semestre,
				Iif(tur.cd_tipo_turma in (1,2,3,5),
				CASE
							WHEN ee.cd_etapa_ensino IN (@etapasInfantil1,@etapasInfantil2) THEN--infantil
										@infantil							
							WHEN ee.cd_etapa_ensino IN (@etapasEja1,@etapasEja2) THEN--eja
										@eja
							WHEN ee.cd_etapa_ensino IN (@etapasFundamental1,@etapasFundamental2,@etapasFundamental3,@etapasFundamental4) THEN--fundamental
										@fundamental
							WHEN ee.cd_etapa_ensino IN (@etapasMedio1,@etapasMedio2,@etapasMedio3,@etapasMedio4,@etapasMedio5) THEN--médio
										@medio
							WHEN esc.tp_escola IN (@tiposEscolaInfantil1,@tiposEscolaInfantil2,@tiposEscolaInfantil3,@tiposEscolaInfantil4,@tiposEscolaInfantil5) THEN--infantil
										@infantil
							WHEN esc.tp_escola NOT IN (@tiposEscolaInfantil1,@tiposEscolaInfantil2,@tiposEscolaInfantil3,@tiposEscolaInfantil4,@tiposEscolaInfantil5) and tur.cd_tipo_turma <> 1 THEN--infantil
										@fundamental
				END, 5)                                          AS codmodalidade,*/
				dre.cd_unidade_educacao AS coddre, 
				dre.nm_unidade_educacao AS dre, 
				dre.nm_exibicao_unidade AS dreabrev, 
				vue.nm_unidade_educacao AS ue, 
				vue.nm_exibicao_unidade AS ueabrev, 
				tur.dc_turma_escola AS nometurma, 
				Iif(tur.cd_tipo_turma = 1, se.sg_resumida_serie, '0') AS ano,
				tue.dc_tipo_unidade_educacao AS tipoue, 
				vue.tp_unidade_educacao AS codtipoue, 
				esc.tp_escola AS codtipoescola, 
				tesc.sg_tp_escola AS tipoescola, 
				dtt.qt_hora_duracao AS duracaoturno, 
				tur.cd_tipo_turno AS tipoturno, 
				tur.dt_atualizacao_tabela AS dataatualizacao,
				Iif((se.cd_etapa_ensino = 13) AND (se.cd_modalidade_ensino = 2) , 1, 0) AS ensinoEspecial,
				se.dc_serie_ensino AS serieEnsino
				FROM       dbo.turma_escola tur
					INNER JOIN dbo.tipo_turno t_trn
						ON t_trn.cd_tipo_turno = tur.cd_tipo_turno
					INNER JOIN dbo.duracao_tipo_turno dtt
						ON         t_trn.cd_tipo_turno = dtt.cd_tipo_turno
					AND tur.cd_duracao = dtt.cd_duracao
					INNER JOIN dbo.tipo_periodicidade tper
						ON tur.cd_tipo_periodicidade = tper.cd_tipo_periodicidade
							   -- Unidades Educacionais
					INNER JOIN dbo.v_cadastro_unidade_educacao vue
						ON vue.cd_unidade_educacao = tur.cd_escola
					INNER JOIN dbo.escola esc
						ON esc.cd_escola = vue.cd_unidade_educacao
					INNER JOIN
					(
								SELECT v_ua.cd_unidade_educacao,
											v_ua.nm_unidade_educacao,
											v_ua.nm_exibicao_unidade
								FROM       dbo.unidade_administrativa ua
									INNER JOIN dbo.v_cadastro_unidade_educacao v_ua
										ON v_ua.cd_unidade_educacao = ua.cd_unidade_administrativa
								--WHERE tp_unidade_administrativa = 24
								) dre
				   ON         dre.cd_unidade_educacao = vue.cd_unidade_administrativa_referencia
			   INNER JOIN dbo.tipo_escola tesc
			   ON         esc.tp_escola = tesc.tp_escola
			   AND        esc.tp_dependencia_administrativa = tesc.tp_dependencia_administrativa
			   INNER JOIN dbo.tipo_unidade_educacao tue
			   ON         tue.tp_unidade_educacao = vue.tp_unidade_educacao 
					-- Serie Ensino(turma tipo = 1)
					LEFT JOIN  dbo.serie_turma_escola ste
						ON tur.cd_turma_escola = ste.cd_turma_escola
							AND        ste.dt_fim IS NULL
					LEFT JOIN dbo.serie_ensino se
						ON         se.cd_serie_ensino = ste.cd_serie_ensino
					LEFT JOIN  dbo.etapa_ensino ee
						ON se.cd_etapa_ensino = ee.cd_etapa_ensino
					LEFT JOIN  dbo.serie_turma_grade stg
						ON tur.cd_turma_escola = stg.cd_turma_escola
							AND tur.cd_escola = stg.cd_escola
							AND ste.cd_serie_ensino = stg.cd_serie_ensino
							AND stg.dt_fim IS NULL
					LEFT JOIN dbo.escola_grade egse
					ON         stg.cd_escola_grade = egse.cd_escola_grade
						AND egse.cd_escola = tur.cd_escola
							   -- Programa (turma tipo = 3, 2 , 5)
					LEFT JOIN  dbo.turma_escola_grade_programa tegpro
						ON tegpro.cd_turma_escola = tur.cd_turma_escola
							AND        tegpro.dt_fim IS NULL
					LEFT JOIN dbo.escola_grade egpro
						ON         egpro.cd_escola_grade = tegpro.cd_escola_grade
							AND egpro.cd_escola = tur.cd_escola
					LEFT JOIN  dbo.grade gpro
						ON gpro.cd_grade = egpro.cd_grade
					LEFT JOIN  dbo.grade_componente_curricular gccpro
						ON gpro.cd_grade = gccpro.cd_grade
					LEFT JOIN  dbo.componente_curricular ccpro
						ON gccpro.cd_componente_curricular = ccpro.cd_componente_curricular
					/*WHERE tur.an_letivo = Year(Getdate())
						AND vue.tp_situacao_unidade = 1
						AND tur.st_turma_escola IN ('O','A')
						AND esc.tp_escola IN (@tiposEscola1,@tiposEscola2,@tiposEscola3,@tiposEscola4,@tiposEscola5,@tiposEscola6,@tiposEscola7,@tiposEscola8,@tiposEscola9)
						AND((
									tur.cd_tipo_turma = 1
									AND ee.cd_etapa_ensino IN (@etapas1,@etapas2,@etapas3,@etapas4,@etapas5,@etapas6,@etapas7,@etapas8,@etapas9,@etapas10,@etapas11,@etapas12,@etapas13)
						AND ((ee.cd_etapa_ensino IN (@etapasInfantil1,@etapasInfantil2) and se.cd_serie_ensino in (@seriesEnsinoInfantil1,@seriesEnsinoInfantil2,@seriesEnsinoInfantil3,@seriesEnsinoInfantil4,@seriesEnsinoInfantil5,@seriesEnsinoInfantil6,@seriesEnsinoInfantil7,@seriesEnsinoInfantil8,@seriesEnsinoInfantil9,@seriesEnsinoInfantil10)) or (ee.cd_etapa_ensino not IN (@etapasInfantil1,@etapasInfantil2)))) 
						   OR(
												 tur.cd_tipo_turma = 3
									  AND tur.dt_fim_turma >= Getdate()
									  AND        Year(tur.dt_inicio_turma) = Year(Getdate())
									  AND gccpro.cd_componente_curricular NOT IN (1033,1051,1052,1053,1054,1322)
									  AND EXISTS
												 (
														SELECT 1 
														FROM matricula_turma_escola mte
														WHERE  mte.cd_turma_escola = tegpro.cd_turma_escola) ) 
						   OR((
															tur.cd_tipo_turma IN (2,
																				  5)
												 OR gccpro.cd_componente_curricular IN (1033,1051,1052,1053,1054,1322)) 
									  AND tur.dt_fim_turma >= Getdate()
									  AND        Year(tur.dt_inicio_turma) = Year(Getdate()))
					)
					and dre.cd_unidade_educacao = 108100*/
GO
/****** Object:  View [dbo].[ix_vw_consulta4]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[ix_vw_consulta4] WITH SCHEMABINDING
AS
SELECT 
                    cd_cargo AS CdCargo
		            ,cd_tipo_funcao AS CdTipoFuncao
                    ,Sobreposto
                    ,ComponenteCurricular
		            ,CargoBase
            FROM(
	            SELECT  
		            cargo.cd_cargo
		            ,null AS cd_tipo_funcao
                    ,0 as Sobreposto
                    ,NULL as ComponenteCurricular
		            ,cargoServidor.cd_cargo_base_servidor as CargoBase
	            FROM dbo.v_servidor_cotic servidor
	            INNER JOIN dbo.v_cargo_base_cotic AS cargoServidor ON cargoServidor.CD_SERVIDOR = servidor.cd_servidor
	            INNER JOIN dbo.cargo AS cargo ON cargoServidor.cd_cargo = cargo.cd_cargo
	            INNER JOIN dbo.lotacao_servidor AS lotacao_servidor 
		            ON cargoServidor.cd_cargo_base_servidor = lotacao_servidor.cd_cargo_base_servidor
	            INNER JOIN dbo.v_cadastro_unidade_educacao ue ON lotacao_servidor.cd_unidade_educacao = ue.cd_unidade_educacao
	            LEFT JOIN dbo.escola ON ue.cd_unidade_educacao = escola.cd_escola
	            WHERE  lotacao_servidor.dt_fim IS NULL 
				--AND servidor.cd_registro_funcional = @RF
		            AND ((escola.tp_escola IS NOT NULL AND escola.tp_escola IN (1,3,4,16,2,17,20,28,31)) OR escola.tp_escola IS NULL) --EMEF,EMEFM,EMEBS, CEU EMEF
	            UNION
	            SELECT  
		            cargo.cd_cargo
		            ,null AS cd_tipo_funcao
                    ,1 as Sobreposto
                    ,NULL as ComponenteCurricular
		            ,cargoServidor.cd_cargo_base_servidor as CargoBase
	            FROM dbo.v_servidor_cotic servidor
		            INNER JOIN dbo.v_cargo_base_cotic AS cargoServidor ON cargoServidor.CD_SERVIDOR = servidor.cd_servidor
		            LEFT JOIN dbo.lotacao_servidor AS lotacao_servidor ON cargoServidor.cd_cargo_base_servidor = lotacao_servidor.cd_cargo_base_servidor
		            INNER JOIN dbo.cargo_sobreposto_servidor AS cargo_sobreposto_servidor 
			            ON cargo_sobreposto_servidor.cd_cargo_base_servidor = cargoServidor.cd_cargo_base_servidor
			            AND (cargo_sobreposto_servidor.dt_fim_cargo_sobreposto IS NULL
			            OR cargo_sobreposto_servidor.dt_fim_cargo_sobreposto > GETDATE())
                    INNER JOIN dbo.cargo AS cargo ON cargo_sobreposto_servidor.cd_cargo = cargo.cd_cargo
		            INNER JOIN dbo.v_cadastro_unidade_educacao ue ON cargo_sobreposto_servidor.cd_unidade_local_servico = ue.cd_unidade_educacao
		            LEFT JOIN dbo.escola ON ue.cd_unidade_educacao = escola.cd_escola
	            WHERE  lotacao_servidor.dt_fim IS NULL 
					--AND servidor.cd_registro_funcional = @RF
		            AND ((escola.tp_escola IS NOT NULL AND escola.tp_escola IN (1,3,4,16,2,17,20,28,31)) OR escola.tp_escola IS NULL) --EMEF,EMEFM,EMEBS, CEU EMEF
	            UNION
	            SELECT  
		            cargo.cd_cargo
		            ,null AS cd_tipo_funcao
                    ,0 as Sobreposto
                    ,componente.cd_componente_curricular as ComponenteCurricular
		            ,cargoServidor.cd_cargo_base_servidor as CargoBase
	            FROM dbo.v_servidor_cotic servidor
		            INNER JOIN dbo.v_cargo_base_cotic AS cargoServidor ON cargoServidor.CD_SERVIDOR = servidor.cd_servidor
		            INNER JOIN dbo.cargo AS cargo ON cargoServidor.cd_cargo = cargo.cd_cargo
		            INNER JOIN dbo.atribuicao_aula atribuicao ON atribuicao.cd_cargo_base_servidor = cargoServidor.cd_cargo_base_servidor
                    LEFT JOIN dbo.componente_curricular componente 
	                    ON atribuicao.cd_componente_curricular = componente.cd_componente_curricular
		                AND componente.dt_cancelamento IS NULL 
		            INNER JOIN dbo.v_cadastro_unidade_educacao ue ON atribuicao.cd_unidade_educacao = ue.cd_unidade_educacao
		            LEFT JOIN dbo.escola ON ue.cd_unidade_educacao = escola.cd_escola
	            WHERE   atribuicao.dt_cancelamento IS NULL 
		            --AND servidor.cd_registro_funcional = @RF
		            AND cargoServidor.dt_fim_nomeacao IS NULL
		            AND atribuicao.dt_disponibilizacao_aulas IS  NULL
		            --AND an_atribuicao  = YEAR(GETDATE())
		            AND ((escola.tp_escola IS NOT NULL AND escola.tp_escola IN (1,3,4,16,2,17,20,28,31)) OR escola.tp_escola IS NULL) --EMEF,EMEFM,EMEBS, CEU EMEF
	            UNION
	            SELECT  
		            cargo.cd_cargo
		            ,atividade.cd_tipo_funcao
                    ,0 as Sobreposto
                    ,NULL as ComponenteCurricular
		            ,cargoServidor.cd_cargo_base_servidor as CargoBase
	            FROM dbo.v_servidor_cotic servidor
		            INNER JOIN dbo.v_cargo_base_cotic AS cargoServidor ON cargoServidor.CD_SERVIDOR = servidor.cd_servidor
		            INNER JOIN dbo.cargo AS cargo ON cargoServidor.cd_cargo = cargo.cd_cargo
		            INNER JOIN dbo.funcao_atividade_cargo_servidor atividade ON atividade.cd_cargo_base_servidor = cargoServidor.cd_cargo_base_servidor
		            INNER JOIN dbo.v_cadastro_unidade_educacao ue ON atividade.cd_unidade_local_servico = ue.cd_unidade_educacao
		            LEFT JOIN dbo.escola ON ue.cd_unidade_educacao = escola.cd_escola
	            WHERE   atividade.dt_fim_funcao_atividade IS NULL 
		            -- AND servidor.cd_registro_funcional = @RF
		            AND ((escola.tp_escola IS NOT NULL AND escola.tp_escola IN (1,3,4,16,2,17,20,28,31)) OR escola.tp_escola IS NULL) --EMEF,EMEFM,EMEBS, CEU EMEF
	            ) Cargos
                GROUP BY  
		             cd_cargo 
		            ,cd_tipo_funcao
					,ComponenteCurricular
                    ,Sobreposto
		            ,CargoBase
GO
/****** Object:  Index [ix_cd_aluno]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_aluno] ON [dbo].[aluno]
(
	[cd_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [atribuicao_aula_cd_componente_curricular_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [atribuicao_aula_cd_componente_curricular_IDX] ON [dbo].[atribuicao_aula]
(
	[cd_componente_curricular] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [atribuicao_aula_cd_grade_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [atribuicao_aula_cd_grade_IDX] ON [dbo].[atribuicao_aula]
(
	[cd_grade] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [atribuicao_aula_dt_cancelamento_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [atribuicao_aula_dt_cancelamento_IDX] ON [dbo].[atribuicao_aula]
(
	[dt_cancelamento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [atribuicao_aula_dt_disponibilizacao_aulas_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [atribuicao_aula_dt_disponibilizacao_aulas_IDX] ON [dbo].[atribuicao_aula]
(
	[dt_disponibilizacao_aulas] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_an_atribuicao]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_an_atribuicao] ON [dbo].[atribuicao_aula]
(
	[an_atribuicao] ASC
)
INCLUDE([cd_serie_grade],[cd_cargo_base_servidor]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_ciclo_ensino]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_ciclo_ensino] ON [dbo].[ciclo_ensino]
(
	[cd_ciclo_ensino] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [componente_curricular_dt_cancelamento_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [componente_curricular_dt_cancelamento_IDX] ON [dbo].[componente_curricular]
(
	[dt_cancelamento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_cd_pessoa]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [idx_cd_pessoa] ON [dbo].[contrato_externo]
(
	[cd_pessoa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [escola_grade_cd_grade_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [escola_grade_cd_grade_IDX] ON [dbo].[escola_grade]
(
	[cd_grade] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [in1]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [in1] ON [dbo].[historico_matricula_turma_escola]
(
	[cd_turma_escola] ASC
)
INCLUDE([cd_matricula],[cd_situacao_aluno],[tp_origem_matricula],[cd_operador],[dt_atlz_tab],[nr_chamada_aluno]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_matricula_cd_turma_escola_dt_situacao_aluno_dt_atlz_tab]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_matricula_cd_turma_escola_dt_situacao_aluno_dt_atlz_tab] ON [dbo].[historico_matricula_turma_escola]
(
	[cd_matricula] ASC,
	[cd_turma_escola] ASC,
	[dt_situacao_aluno] ASC,
	[dt_atlz_tab] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_laudo_medico]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_laudo_medico] ON [dbo].[laudo_medico]
(
	[cd_laudo_medico] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_laudo_medico_cd_cargo_base_servidor]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [IX_laudo_medico_cd_cargo_base_servidor] ON [dbo].[laudo_medico]
(
	[cd_cargo_base_servidor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_municipio_cd_municipio]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [IX_municipio_cd_municipio] ON [dbo].[municipio]
(
	[cd_municipio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_identificador_necessidade_especial_aluno]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_identificador_necessidade_especial_aluno] ON [dbo].[necessidade_especial_aluno]
(
	[cd_identificador_necessidade_especial_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_cd_cpf_pessoa]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [idx_cd_cpf_pessoa] ON [dbo].[pessoa]
(
	[cd_cpf_pessoa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_aluno]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_aluno] ON [dbo].[responsavel_aluno]
(
	[cd_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_identificador_responsavel]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_identificador_responsavel] ON [dbo].[responsavel_aluno]
(
	[cd_identificador_responsavel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cpf_responsavel]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cpf_responsavel] ON [dbo].[responsavel_aluno]
(
	[cd_cpf_responsavel] ASC
)
WHERE ([cd_cpf_responsavel] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_cd_escola_cd_serie_ensino]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_escola_cd_serie_ensino] ON [dbo].[serie_escola]
(
	[cd_escola] ASC,
	[cd_serie_ensino] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_cd_turma]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [idx_cd_turma] ON [dbo].[serie_turma_grade]
(
	[cd_turma_escola] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [serie_turma_grade_cd_escola_grade_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [serie_turma_grade_cd_escola_grade_IDX] ON [dbo].[serie_turma_grade]
(
	[cd_escola_grade] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_sub_prefeitura]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_sub_prefeitura] ON [dbo].[sub_prefeitura]
(
	[cd_sub_prefeitura] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_territorio_saber]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_territorio_saber] ON [dbo].[territorio_saber]
(
	[cd_territorio_saber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_experiencia_pedagogica]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_experiencia_pedagogica] ON [dbo].[tipo_experiencia_pedagogica]
(
	[cd_experiencia_pedagogica] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [turma_escola_cd_escola_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [turma_escola_cd_escola_IDX] ON [dbo].[turma_escola]
(
	[cd_escola] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [turma_escola_grade_programa_cd_escola_grade_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [turma_escola_grade_programa_cd_escola_grade_IDX] ON [dbo].[turma_escola_grade_programa]
(
	[cd_escola_grade] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [turma_escola_grade_programa_cd_turma_escola_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [turma_escola_grade_programa_cd_turma_escola_IDX] ON [dbo].[turma_escola_grade_programa]
(
	[cd_turma_escola] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_serie_grade_cd_componente_curricular_cd_territorio_saber_cd_experiencia_pedagogica]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_serie_grade_cd_componente_curricular_cd_territorio_saber_cd_experiencia_pedagogica] ON [dbo].[turma_grade_territorio_experiencia]
(
	[cd_serie_grade] ASC,
	[cd_componente_curricular] ASC,
	[cd_territorio_saber] ASC,
	[cd_experiencia_pedagogica] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_aluno]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_aluno] ON [dbo].[v_aluno_cotic]
(
	[cd_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [v_aluno_cotic_nm_aluno_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [v_aluno_cotic_nm_aluno_IDX] ON [dbo].[v_aluno_cotic]
(
	[nm_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [v_cargo_base_cotic_cd_cargo_base_servidor_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [v_cargo_base_cotic_cd_cargo_base_servidor_IDX] ON [dbo].[v_cargo_base_cotic]
(
	[cd_cargo_base_servidor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [v_cargo_base_cotic_cd_servidor_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [v_cargo_base_cotic_cd_servidor_IDX] ON [dbo].[v_cargo_base_cotic]
(
	[cd_servidor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_an_letivo]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_an_letivo] ON [dbo].[v_historico_matricula_cotic]
(
	[an_letivo] ASC
)
INCLUDE([cd_matricula],[dt_status_matricula],[st_matricula],[cd_aluno]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_an_letivo_cd_aluno_cd_serie_ensino_cd_matricula]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_an_letivo_cd_aluno_cd_serie_ensino_cd_matricula] ON [dbo].[v_historico_matricula_cotic]
(
	[an_letivo] ASC,
	[cd_aluno] ASC,
	[cd_serie_ensino] ASC,
	[cd_matricula] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_aluno]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_aluno] ON [dbo].[v_historico_matricula_cotic]
(
	[cd_aluno] ASC
)
INCLUDE([cd_matricula]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_cd_matricula_cd_aluno]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_matricula_cd_aluno] ON [dbo].[v_historico_matricula_cotic]
(
	[cd_matricula] ASC,
	[cd_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_cd_matricula_dt_status_matricula_st_matricula_tp_matricula]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_matricula_dt_status_matricula_st_matricula_tp_matricula] ON [dbo].[v_historico_matricula_cotic]
(
	[cd_matricula] ASC,
	[dt_status_matricula] ASC,
	[st_matricula] ASC,
	[tp_matricula] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [v_matricula_cotic_an_letivo_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [v_matricula_cotic_an_letivo_IDX] ON [dbo].[v_matricula_cotic]
(
	[an_letivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [v_matricula_cotic_cd_aluno_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [v_matricula_cotic_cd_aluno_IDX] ON [dbo].[v_matricula_cotic]
(
	[cd_aluno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [v_servidor_cotic_cd_registro_funcional_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [v_servidor_cotic_cd_registro_funcional_IDX] ON [dbo].[v_servidor_cotic]
(
	[cd_registro_funcional] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [v_servidor_cotic_cd_servidor_IDX]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [v_servidor_cotic_cd_servidor_IDX] ON [dbo].[v_servidor_cotic]
(
	[cd_servidor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_cd_servidor_dc_dispositivo_dt_inicio_dt_fim]    Script Date: 03/05/2021 10:48:57 ******/
CREATE NONCLUSTERED INDEX [ix_cd_servidor_dc_dispositivo_dt_inicio_dt_fim] ON [dbo].[v_servidor_email_cotic]
(
	[cd_servidor] ASC,
	[dc_dispositivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[componente_curricular] ADD  CONSTRAINT [DF1_componente_curricular]  DEFAULT ('N') FOR [in_sp_integral]
GO
ALTER TABLE [dbo].[matricula_turma_escola] ADD  CONSTRAINT [DF_matricula_turma_escola]  DEFAULT (getdate()) FOR [dt_atlz_tab]
GO
ALTER TABLE [dbo].[serie_ensino] ADD  CONSTRAINT [DF1_serie_ensino]  DEFAULT ('S') FOR [in_atribuicao_aula]
GO
ALTER TABLE [dbo].[tipo_escola] ADD  CONSTRAINT [DF1_tipo_escola]  DEFAULT ('N') FOR [in_uniforme_escolar]
GO
/****** Object:  StoredProcedure [dbo].[proc_GetStepFailureData]    Script Date: 03/05/2021 10:48:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[proc_GetStepFailureData]
(
@JobName VARCHAR(250)
)
AS
/*
This procedure gets failure log data for the failed step of a SQL Server Agent job
*/
DECLARE @job_id UNIQUEIDENTIFIER
SELECT @job_id = job_id FROM dbo.sysjobs WHERE [name] = @JobName
SELECT 'Step ' + CAST(JH.step_id AS VARCHAR(3)) + ' of ' + (SELECT CAST(COUNT(*) AS VARCHAR(5)) FROM dbo.sysjobsteps WHERE job_id = @job_id) AS StepFailed,
 CAST(RIGHT(JH.run_date,2) AS CHAR(2)) + '/' + CAST(SUBSTRING(CAST(JH.run_date AS CHAR(8)),5,2) AS CHAR(2)) + '/' + CAST(LEFT(JH.run_date,4) AS CHAR(4)) AS DateRun,
 LEFT(RIGHT('0' + CAST(JH.run_time AS VARCHAR(6)),6),2) + ':' + SUBSTRING(RIGHT('0' + CAST(JH.run_time AS VARCHAR(6)),6),3,2) + ':' + LEFT(RIGHT('0' + CAST(JH.run_time AS VARCHAR(6)),6),2) AS TimeRun,
 JS.step_name, 
 JH.run_duration, 
 CASE
 WHEN JSL.[log] IS NULL THEN JH.[Message]
 ELSE JSL.[log]
 END AS LogOutput
FROM dbo.sysjobsteps JS INNER JOIN dbo.sysjobhistory JH 
 ON JS.job_id = JH.job_id AND JS.step_id = JH.step_id 
 LEFT OUTER JOIN dbo.sysjobstepslogs JSL
 ON JS.step_uid = JSL.step_uid
WHERE INSTANCE_ID >
 (SELECT MIN(INSTANCE_ID)
 FROM (
 SELECT top (2) INSTANCE_ID, job_id
 FROM dbo.sysjobhistory
 WHERE job_id = @job_id
 AND STEP_ID = 0
 ORDER BY INSTANCE_ID desc
 ) A
 )
 AND JS.step_id <> 0 
 AND JH.job_id = @job_id
 AND JH.run_status = 0
ORDER BY JS.step_id ASC
GO
USE [master]
GO
ALTER DATABASE [se1426] SET  READ_WRITE 
GO
