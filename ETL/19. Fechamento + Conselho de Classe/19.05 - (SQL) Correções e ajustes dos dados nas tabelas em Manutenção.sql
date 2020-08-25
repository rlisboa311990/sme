USE GestaoPedagogica
GO


-- CORRIGINDO A S�RIE
TRUNCATE TABLE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC_508]
GO
 
INSERT INTO [Manutencao].[dbo].[ETL_SGP_Fechamento_CC_508]
SELECT DISTINCT
       ETL.[TURMA_ID],
       ETL.[PERIODO_ESCOLAR_ID],
       ETL.[CRIADO_EM],
       ETL.[ALTERADO_EM],
       ETL.[DISCIPLINA_ID],
       ETL.[ALUNO_CODIGO],
       ETL.[NOTA],
	   ETL.[SINTESE_ID],
	   ETL.[CONCEITO_ID],
	   ETL.[CRIADO_EM_CC],
	   ETL.[ALTERADO_EM_CC],
	   ETL.[RECOMENDACOES_ALUNO],
	   ETL.[RECOMENDACOES_FAMILIA],
	   ETL.[ANOTACOES_PEDAGOGICAS],
	   ETL.[NOTACC],
	   ETL.[JUSTIFICATIVA],
	   ETL.[PARECER],
	   [dbo].[fn_TiraLetras](FTE.dc_turma_escola) AS [SERIE],
	   ETL.[CONCEITO_ID_CC]
  FROM            [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]            AS ETL (NOLOCK) 
       INNER JOIN [Manutencao].[dbo].[ETL_SGP_Fechamento_turma_escola]  AS FTE (NOLOCK)
               ON FTE.cd_turma_escola = ETL.TURMA_ID
GO

DELETE
  FROM [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
GO

INSERT INTO [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
SELECT ETL.[TURMA_ID],
       ETL.[PERIODO_ESCOLAR_ID],
       ETL.[CRIADO_EM],
       ETL.[ALTERADO_EM],
       ETL.[DISCIPLINA_ID],
       ETL.[ALUNO_CODIGO],
       ETL.[NOTA],
	   ETL.[SINTESE_ID],
	   ETL.[CONCEITO_ID],
	   ETL.[CRIADO_EM_CC],
	   ETL.[ALTERADO_EM_CC],
	   ETL.[RECOMENDACOES_ALUNO],
	   ETL.[RECOMENDACOES_FAMILIA],
	   ETL.[ANOTACOES_PEDAGOGICAS],
	   ETL.[NOTACC],
	   ETL.[JUSTIFICATIVA],
	   ETL.[PARECER],
	   ETL.[SERIE],
	   ETL.[CONCEITO_ID_CC]
  FROM [Manutencao].[dbo].[ETL_SGP_Fechamento_CC_508] AS ETL (NOLOCK) 
GO



-- CORRIGINDO DISCIPLINAS DE REG�NCIA DE CLASSE
TRUNCATE TABLE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC_508]
GO
 
INSERT INTO [Manutencao].[dbo].[ETL_SGP_Fechamento_CC_508]
SELECT DISTINCT
       ETL.[TURMA_ID],
       ETL.[PERIODO_ESCOLAR_ID],
       ETL.[CRIADO_EM],
       ETL.[ALTERADO_EM],
       GCC.cd_componente_curricular AS [DISCIPLINA_ID],
       ETL.[ALUNO_CODIGO],
       ETL.[NOTA],
	   ETL.[SINTESE_ID],
	   ETL.[CONCEITO_ID],
	   ETL.[CRIADO_EM_CC],
	   ETL.[ALTERADO_EM_CC],
	   ETL.[RECOMENDACOES_ALUNO],
	   ETL.[RECOMENDACOES_FAMILIA],
	   ETL.[ANOTACOES_PEDAGOGICAS],
	   ETL.[NOTACC],
	   ETL.[JUSTIFICATIVA],
	   ETL.[PARECER],
	   ETL.[SERIE],
	   ETL.[CONCEITO_ID_CC]
  FROM            [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]                          AS ETL (NOLOCK) 
       INNER JOIN [Manutencao].[dbo].[ETL_SGP_Fechamento_serie_turma_grade]           AS STG (NOLOCK) 
               ON STG.cd_turma_escola = ETL.TURMA_ID
              AND STG.dt_fim IS NULL
       INNER JOIN [Manutencao].[dbo].[ETL_SGP_Fechamento_escola_grade]                AS EGR (NOLOCK)
               ON EGR.cd_escola_grade = STG.cd_escola_grade
       INNER JOIN [Manutencao].[dbo].[ETL_SGP_Fechamento_grade_componente_curricular] AS GCC (NOLOCK) 
               ON GCC.cd_grade = EGR.cd_grade
              AND GCC.cd_componente_curricular IN (508, 511, 1064, 1065, 1104, 1105, 1112, 1113, 1114, 1115, 1117, 1121, 1124, 1125, 1211, 1212, 1213, 1290, 1301)
 WHERE ETL.DISCIPLINA_ID IN (508, 511, 1064, 1065, 1104, 1105, 1112, 1113, 1114, 1115, 1117, 1121, 1124, 1125, 1211, 1212, 1213, 1290, 1301)
GO

DELETE
  FROM [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
 WHERE [DISCIPLINA_ID] IN (508, 511, 1064, 1065, 1104, 1105, 1112, 1113, 1114, 1115, 1117, 1121, 1124, 1125, 1211, 1212, 1213, 1290, 1301)
GO

INSERT INTO [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
SELECT DISTINCT
       ETL.[TURMA_ID],
       ETL.[PERIODO_ESCOLAR_ID],
       ETL.[CRIADO_EM],
       ETL.[ALTERADO_EM],
       ETL.[DISCIPLINA_ID],
       ETL.[ALUNO_CODIGO],
       ETL.[NOTA],
	   ETL.[SINTESE_ID],
	   ETL.[CONCEITO_ID],
	   ETL.[CRIADO_EM_CC],
	   ETL.[ALTERADO_EM_CC],
	   ETL.[RECOMENDACOES_ALUNO],
	   ETL.[RECOMENDACOES_FAMILIA],
	   ETL.[ANOTACOES_PEDAGOGICAS],
	   ETL.[NOTACC],
	   ETL.[JUSTIFICATIVA],
	   ETL.[PARECER],
	   ETL.[SERIE],
	   ETL.[CONCEITO_ID_CC]
  FROM [Manutencao].[dbo].[ETL_SGP_Fechamento_CC_508] AS ETL (NOLOCK) 
GO



-- CORRIGINDO DISCIPLINAS DE INGL�S
UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [DISCIPLINA_ID] = 9
 WHERE [DISCIPLINA_ID] = 1046
GO


-- TRUNCANDO CAMPOS GRANDES
UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_ALUNO] = LEFT([RECOMENDACOES_ALUNO], 30000)
 WHERE LEN([RECOMENDACOES_ALUNO]) >= 30000
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_FAMILIA] = LEFT([RECOMENDACOES_FAMILIA], 30000)
 WHERE LEN([RECOMENDACOES_FAMILIA]) >= 30000
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [ANOTACOES_PEDAGOGICAS] = LEFT([ANOTACOES_PEDAGOGICAS], 30000)
 WHERE LEN([ANOTACOES_PEDAGOGICAS]) >= 30000
GO  

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [JUSTIFICATIVA] = LEFT([JUSTIFICATIVA], 30000)
 WHERE LEN([JUSTIFICATIVA]) >= 30000
GO  


-- CORRIGINDO SUJEIRAS (LTRIM/RTRIM)
UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_ALUNO] = LTRIM(RTRIM([RECOMENDACOES_ALUNO]))
 WHERE [RECOMENDACOES_ALUNO] IS NOT NULL
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_FAMILIA] = LTRIM(RTRIM([RECOMENDACOES_FAMILIA]))
 WHERE [RECOMENDACOES_FAMILIA] IS NOT NULL
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [ANOTACOES_PEDAGOGICAS] = LTRIM(RTRIM([ANOTACOES_PEDAGOGICAS]))
 WHERE [ANOTACOES_PEDAGOGICAS] IS NOT NULL
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [JUSTIFICATIVA] = LTRIM(RTRIM([JUSTIFICATIVA]))
 WHERE [JUSTIFICATIVA] IS NOT NULL
GO


-- CORRIGINDO SUJEIRAS (-)
UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_ALUNO] = SUBSTRING([RECOMENDACOES_ALUNO], 2,LEN([RECOMENDACOES_ALUNO]))
 WHERE LEFT([RECOMENDACOES_ALUNO],1) = '-'
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_FAMILIA] = SUBSTRING([RECOMENDACOES_FAMILIA], 2,LEN([RECOMENDACOES_FAMILIA]))
 WHERE LEFT([RECOMENDACOES_FAMILIA],1) = '-'
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [ANOTACOES_PEDAGOGICAS] = SUBSTRING([ANOTACOES_PEDAGOGICAS], 2,LEN([ANOTACOES_PEDAGOGICAS]))
 WHERE LEFT([ANOTACOES_PEDAGOGICAS],1) = '-'
GO  

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [JUSTIFICATIVA] = SUBSTRING([JUSTIFICATIVA], 2,LEN([JUSTIFICATIVA]))
 WHERE LEFT([JUSTIFICATIVA],1) = '-'
GO  


-- CORRIGINDO SUJEIRAS (|)
UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_ALUNO] = REPLACE([RECOMENDACOES_ALUNO], '|', ' ')
 WHERE [RECOMENDACOES_ALUNO] LIKE '%|%'
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_FAMILIA] = REPLACE([RECOMENDACOES_FAMILIA], '|', ' ')
 WHERE [RECOMENDACOES_FAMILIA] LIKE '%|%'
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [ANOTACOES_PEDAGOGICAS] = REPLACE([ANOTACOES_PEDAGOGICAS], '|', ' ')
 WHERE [ANOTACOES_PEDAGOGICAS] LIKE '%|%'
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [JUSTIFICATIVA] = REPLACE([JUSTIFICATIVA], '|', ' ')
 WHERE [JUSTIFICATIVA] LIKE '%|%'
GO


-- CORRIGINDO SUJEIRAS (;)
UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_ALUNO] = REPLACE([RECOMENDACOES_ALUNO], ';', ' ')
 WHERE [RECOMENDACOES_ALUNO] LIKE '%;%'
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_FAMILIA] = REPLACE([RECOMENDACOES_FAMILIA], ';', ' ')
 WHERE [RECOMENDACOES_FAMILIA] LIKE '%;%'
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [ANOTACOES_PEDAGOGICAS] = REPLACE([ANOTACOES_PEDAGOGICAS], ';', ' ')
 WHERE [ANOTACOES_PEDAGOGICAS] LIKE '%;%'
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [JUSTIFICATIVA] = REPLACE([JUSTIFICATIVA], ';', ' ')
 WHERE [JUSTIFICATIVA] LIKE '%;%'
GO


-- CORRIGINDO SUJEIRAS (NULL)
UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_ALUNO] = 'Migrado - N�o informado no legado.'
 WHERE [RECOMENDACOES_ALUNO] IS NULL
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_FAMILIA] = 'Migrado - N�o informado no legado.'
 WHERE [RECOMENDACOES_FAMILIA] IS NULL
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [ANOTACOES_PEDAGOGICAS] = 'Migrado - N�o informado no legado.'
 WHERE [ANOTACOES_PEDAGOGICAS] IS NULL
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [JUSTIFICATIVA] = 'Migrado - N�o informado no legado.'
 WHERE [JUSTIFICATIVA] IS NULL
GO


-- CORRIGINDO SUJEIRAS (LTRIM/RTRIM)
UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_ALUNO] = LTRIM(RTRIM([RECOMENDACOES_ALUNO]))
 WHERE [RECOMENDACOES_ALUNO] IS NOT NULL
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [RECOMENDACOES_FAMILIA] = LTRIM(RTRIM([RECOMENDACOES_FAMILIA]))
 WHERE [RECOMENDACOES_FAMILIA] IS NOT NULL
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [ANOTACOES_PEDAGOGICAS] = LTRIM(RTRIM([ANOTACOES_PEDAGOGICAS]))
 WHERE [ANOTACOES_PEDAGOGICAS] IS NOT NULL
GO

UPDATE [Manutencao].[dbo].[ETL_SGP_Fechamento_CC]
   SET [JUSTIFICATIVA] = LTRIM(RTRIM([JUSTIFICATIVA]))
 WHERE [JUSTIFICATIVA] IS NOT NULL
GO
