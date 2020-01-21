pipeline {
    agent {
      node { 
        label 'dockerdotnet'
      }
    }
    
    options {
      buildDiscarder(logRotator(numToKeepStr: '5', artifactNumToKeepStr: '5'))
      disableConcurrentBuilds()
      skipDefaultCheckout()  
    }
    
        
    stages {
      stage('CheckOut') {
        steps {
          
          checkout scm  
        }
      }
      
 
         
      stage('Build projeto') {
        steps {
          sh "echo executando build de projeto"
          sh 'dotnet build'
        }
      }
        
             
      stage('Analise Codigo') {
          when {
                branch 'release'
            }
         steps {
             sh 'echo Analise SonarQube API'
             sh 'dotnet-sonarscanner begin /k:"SME-NovoSGP" /d:sonar.host.url="http://automation.educacao.intranet:9000" /d:sonar.login="346fd763d9581684b9271a03d8ef5a16fe92622b"'
             sh 'dotnet build'
             sh 'dotnet-sonarscanner end /d:sonar.login="346fd763d9581684b9271a03d8ef5a16fe92622b"'
           // anlise de frontend
             sh 'echo Analise SonarQube FRONTEND'
             sh 'sonar-scanner \
               -Dsonar.projectKey=SME-NovoSGP-WebClient \
               -Dsonar.sources=src/SME.SGP.WebClient \
               -Dsonar.host.url=http://sonar.sme.prefeitura.sp.gov.br \
               -Dsonar.login=1ab3b0eb51a0f51c846c13f2f5a0255fd5d7583e'
         }
       }
      
         stage('Testes de integração') {
        steps {
          
          //Execuita os testes gerando um relatorio formato trx
            //sh 'dotnet test --logger "trx;LogFileName=TestResults.trx"'
            sh 'echo executando testes'
          //Publica o relatorio de testes
           // mstest()
          
        }
     }
        
      stage('Deploy DEV') {
            when {
                branch 'development'
            }
            steps {
                 
                 sh 'echo Deploying desenvolvimento'
                
        // Start JOB Rundeck para build das imagens Docker e push SME Registry
      
          script {
           step([$class: "RundeckNotifier",
              includeRundeckLogs: true,
                               
              //JOB DE BUILD
              jobId: "743ccbae-bd30-4ac6-b2a3-2f0d1c64e937",
              nodeFilters: "",
              //options: """
              //     PARAM_1=value1
               //    PARAM_2=value2
              //     PARAM_3=
              //     """,
              rundeckInstance: "Rundeck-SME",
              shouldFailTheBuild: true,
              shouldWaitForRundeckJob: true,
              tags: "",
              tailLog: true])
           }
                
       //Start JOB Rundeck para update de deploy Kubernetes DEV
         
         script {
            step([$class: "RundeckNotifier",
              includeRundeckLogs: true,
              jobId: "f6c3e74c-6411-466a-84a7-921d637c2645",
              nodeFilters: "",
              //options: """
              //     PARAM_1=value1
               //    PARAM_2=value2
              //     PARAM_3=
              //     """,
              rundeckInstance: "Rundeck-SME",
              shouldFailTheBuild: true,
              shouldWaitForRundeckJob: true,
              tags: "",
              tailLog: true])
           }
      
       
            }
        }
        
        stage('Deploy QA') {
            when {
                branch 'development'
            }
            steps {
                sh 'echo Deploying QA'
            }
        }
      
      stage('Deploy homologacao') {
            when {
                branch 'release'
            }
            steps {
                 timeout(time: 24, unit: "HOURS") {
               
                 telegramSend("${JOB_NAME}...O Build ${BUILD_DISPLAY_NAME} - Requer uma aprovação para deploy !!!\n Consulte o log para detalhes -> [Job logs](${env.BUILD_URL}console)\n")
                 input message: 'Deseja realizar o deploy?', ok: 'SIM', submitter: 'marcos_costa,danieli_paula,everton_nogueira'
            }
                 sh 'echo Deploying homologacao'
                
        // Start JOB Rundeck para build das imagens Docker e push registry SME
      
          script {
           step([$class: "RundeckNotifier",
              includeRundeckLogs: true,
                
               
              //JOB DE BUILD
              jobId: "397ce3f8-0af7-4d26-b65b-19f09ccf6c82",
              nodeFilters: "",
              //options: """
              //     PARAM_1=value1
               //    PARAM_2=value2
              //     PARAM_3=
              //     """,
              rundeckInstance: "Rundeck-SME",
              shouldFailTheBuild: true,
              shouldWaitForRundeckJob: true,
              tags: "",
              tailLog: true])
           }
                
       //Start JOB Rundeck para update de imagens no host homologação 
         
         script {
            step([$class: "RundeckNotifier",
              includeRundeckLogs: true,
              jobId: "ec4238e5-4aab-4b5d-b949-aa46d6b2b09d",
              nodeFilters: "",
              //options: """
              //     PARAM_1=value1
               //    PARAM_2=value2
              //     PARAM_3=
              //     """,
              rundeckInstance: "Rundeck-SME",
              shouldFailTheBuild: true,
              shouldWaitForRundeckJob: true,
              tags: "",
              tailLog: true])
           }
      
       
            }
        }

        stage('Deploy produção') {
            when {
                branch 'master'
            }
            steps {
                 timeout(time: 24, unit: "HOURS") {
               
                 telegramSend("${JOB_NAME}...O Build ${BUILD_DISPLAY_NAME} - Requer uma aprovação para deploy !!!\n Consulte o log para detalhes -> [Job logs](${env.BUILD_URL}console)\n")
                 input message: 'Deseja realizar o deploy?', ok: 'SIM', submitter: 'marcos_costa,danieli_paula,everton_nogueira'
            }
                 sh 'echo Deploy produção'
                
        // Start JOB Rundeck para build das imagens Docker e push registry SME
      
          script {
           step([$class: "RundeckNotifier",
              includeRundeckLogs: true,
            
               
              //JOB DE BUILD
              jobId: "b6ff0cbf-6267-41af-bb56-5cdc3eb86902",
              nodeFilters: "",
              //options: """
              //     PARAM_1=value1
               //    PARAM_2=value2
              //     PARAM_3=
              //     """,
              rundeckInstance: "Rundeck-SME",
              shouldFailTheBuild: true,
              shouldWaitForRundeckJob: true,
              tags: "",
              tailLog: true])
           }
                
       //Start JOB Rundeck para deploy em produção 
         
         script {
            step([$class: "RundeckNotifier",
              includeRundeckLogs: true,
              jobId: "6a3d314b-672b-4fe3-9759-0b08847eb27e",
              nodeFilters: "",
              //options: """
              //     PARAM_1=value1
               //    PARAM_2=value2
              //     PARAM_3=
              //     """,
              rundeckInstance: "Rundeck-SME",
              shouldFailTheBuild: true,
              shouldWaitForRundeckJob: true,
              tags: "",
              tailLog: true])
           }
      
       
            }
        }
     
}

    
post {
        always {
            //step ([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: false, keepLongStdio: true])
            echo 'One way or another, I have finished'
            
            
        }
        success {
           // withCredentials([string(credentialsId: 'webhook-backend', variable: 'WH-teams')]) {
           //   office365ConnectorSend color: '008000', message: "O Build ${BUILD_DISPLAY_NAME} - Esta ok !!!  <${env.BUILD_URL}> ", status: 'SUCESSO', webhookUrl: '$WH-teams'
           // }
            telegramSend("${JOB_NAME}...O Build ${BUILD_DISPLAY_NAME} - Esta ok !!!\n Consulte o log para detalhes -> [Job logs](${env.BUILD_URL}console)\n\n Uma nova versão da aplicação esta disponivel!!!")
        }
        unstable {
           // withCredentials([string(credentialsId: 'webhook-backend', variable: 'WH-teams')]) {
           //  office365ConnectorSend color: 'ffa500', message: "O Build ${BUILD_DISPLAY_NAME} <${env.BUILD_URL}> - Esta instavel ...Verifique os logs para corrigir o problema'", status: 'INSTAVEL', webhookUrl: '$WH-teams'
           //}
            telegramSend("O Build ${BUILD_DISPLAY_NAME} <${env.BUILD_URL}> - Esta instavel ...\nConsulte o log para detalhes -> [Job logs](${env.BUILD_URL}console)")
        }
        failure {
            // withCredentials([string(credentialsId: 'webhook-backend', variable: 'WH-teams')]) {
            //   office365ConnectorSend color: 'd00000', message: "O Build ${BUILD_DISPLAY_NAME} <${env.BUILD_URL}> - Quebrou. Verifique os logs para corrigir o problema'", status: 'FALHOU', webhookUrl: '$WH-teams'
            // }
             telegramSend("${JOB_NAME}...O Build ${BUILD_DISPLAY_NAME}  - Quebrou. \nConsulte o log para detalhes -> [Job logs](${env.BUILD_URL}console)")
        }
        changed {
             //withCredentials([string(credentialsId: 'webhook-backend', variable: 'WH-teams')]) {
               echo 'Things were different before...'
            // }
        }
       aborted {
             //withCredentials([string(credentialsId: 'webhook-API', variable: 'WHapi-teams')]) {
             //  office365ConnectorSend color: 'd00000', message: "O Build ${BUILD_DISPLAY_NAME} <${env.BUILD_URL}> - Quebrou. Verifique os logs para corrigir o problema'", status: 'FALHOU', webhookUrl: '$WHapi-teams'
             //}
             telegramSend("O Build ${BUILD_DISPLAY_NAME} - Foi abortado.\nConsulte o log para detalhes -> [Job logs](${env.BUILD_URL}console)")
        }
    }
}
