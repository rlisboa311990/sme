pipeline {
    environment {
      branchname =  env.BRANCH_NAME.toLowerCase()
      kubeconfig = getKubeconf(env.branchname)
      registryCredential = 'jenkins_registry'
    }
  
    agent {
      node { label 'dotnet-3-rc' }
    }

    options {
      buildDiscarder(logRotator(numToKeepStr: '5', artifactNumToKeepStr: '5'))
      disableConcurrentBuilds()
      skipDefaultCheckout()
    }
  
    stages {

        stage('CheckOut') {            
            steps { checkout scm }            
        }

        stage('BuildProjeto') {
          steps {
            sh "echo executando build"
            sh 'dotnet build'
          }
        }
      
        //stage('AnaliseCodigo') {
	     //   when { branch 'release' }
         // steps {
         //     withSonarQubeEnv('sonarqube-local'){
         //       sh 'dotnet-sonarscanner begin /k:"SME-NovoSGP-API-EOL"'
         //       sh 'dotnet build SME.Pedagogico.API.sln'
         //       sh 'dotnet-sonarscanner'
         //   }
         // }
       // }

        stage('Build') {
          when { anyOf { branch 'master'; branch 'main'; branch "story/*"; branch 'development'; branch 'release';  } } 
          steps {
            script {
              imagename1 = "registry.sme.prefeitura.sp.gov.br/${env.branchname}/sme-sgp-backend"
              imagename2 = "registry.sme.prefeitura.sp.gov.br/${env.branchname}/sme-worker-rabbit"
              imagename3 = "registry.sme.prefeitura.sp.gov.br/${env.branchname}/sme-workerservice"              
              dockerImage1 = docker.build(imagename1, "-f src/SME.SGP.Api/Dockerfile .")
              dockerImage2 = docker.build(imagename2, "-f src/SME.SGP.Worker.Rabbbit/Dockerfile .")
              dockerImage2 = docker.build(imagename3, "-f src/SME.SGP.WorkerService/Dockerfile .")
              docker.withRegistry( 'https://registry.sme.prefeitura.sp.gov.br', registryCredential ) {
              dockerImage1.push()
              dockerImage2.push()
              dockerImage3.push()
              }
              sh "docker rmi $imagename1"
              sh "docker rmi $imagename2"
              sh "docker rmi $imagename3"
            }
          }
        }
	    
        stage('Deploy'){
            when { anyOf {  branch 'master'; branch 'main'; branch 'development'; branch 'release'; } }        
            steps {
                script{
                    if ( env.branchname == 'main' ||  env.branchname == 'master' || env.branchname == 'homolog' || env.branchname == 'release' ) {
                        sendTelegram("🤩 [Deploy ${env.branchname}] Job Name: ${JOB_NAME} \nBuild: ${BUILD_DISPLAY_NAME} \nMe aprove! \nLog: \n${env.BUILD_URL}")
                        timeout(time: 24, unit: "HOURS") {
                            input message: 'Deseja realizar o deploy?', ok: 'SIM', submitter: 'marlon_goncalves, bruno_alevato, robson_silva, rafael_losi'
                        }
                        withCredentials([file(credentialsId: "${kubeconfig}", variable: 'config')]){
                            sh('cp $config '+"$home"+'/.kube/config')
                            sh 'kubectl rollout restart deployment/sme-api -n sme-novosgp'
                            sh 'kubectl rollout restart deployment/sme-pedagogico-worker -n sme-novosgp'
                            sh 'kubectl rollout restart deployment/sme-workerservice -n sme-novosgp'                            
                            sh('rm -f '+"$home"+'/.kube/config')
                        }
                    }
                    else{
                        withCredentials([file(credentialsId: "${kubeconfig}", variable: 'config')]){
                            sh('cp $config '+"$home"+'/.kube/config')
                            sh 'kubectl rollout restart deployment/sme-api -n sme-novosgp'
                            sh 'kubectl rollout restart deployment/sme-pedagogico-worker -n sme-novosgp'
                            sh 'kubectl rollout restart deployment/sme-workerservice -n sme-novosgp'  
                            sh('rm -f '+"$home"+'/.kube/config')
                        }
                    }
                }
            }           
        }    
    }

  post {
    success { sendTelegram("🚀 Job Name: ${JOB_NAME} \nBuild: ${BUILD_DISPLAY_NAME} \nStatus: Success \nLog: \n${env.BUILD_URL}console") }
    unstable { sendTelegram("💣 Job Name: ${JOB_NAME} \nBuild: ${BUILD_DISPLAY_NAME} \nStatus: Unstable \nLog: \n${env.BUILD_URL}console") }
    failure { sendTelegram("💥 Job Name: ${JOB_NAME} \nBuild: ${BUILD_DISPLAY_NAME} \nStatus: Failure \nLog: \n${env.BUILD_URL}console") }
    aborted { sendTelegram ("😥 Job Name: ${JOB_NAME} \nBuild: ${BUILD_DISPLAY_NAME} \nStatus: Aborted \nLog: \n${env.BUILD_URL}console") }
  }
}
def sendTelegram(message) {
    def encodedMessage = URLEncoder.encode(message, "UTF-8")
    withCredentials([string(credentialsId: 'telegramToken', variable: 'TOKEN'),
    string(credentialsId: 'telegramChatId', variable: 'CHAT_ID')]) {
        response = httpRequest (consoleLogResponseBody: true,
                contentType: 'APPLICATION_JSON',
                httpMode: 'GET',
                url: 'https://api.telegram.org/bot'+"$TOKEN"+'/sendMessage?text='+encodedMessage+'&chat_id='+"$CHAT_ID"+'&disable_web_page_preview=true',
                validResponseCodes: '200')
        return response
    }
}
def getKubeconf(branchName) {
    if("main".equals(branchName)) { return "config_prd"; }
    else if ("master".equals(branchName)) { return "config_prd"; }
    else if ("homolog".equals(branchName)) { return "config_hom"; }
    else if ("release".equals(branchName)) { return "config_hom"; }
    else if ("development".equals(branchName)) { return "config_dev"; }
    else if ("develop".equals(branchName)) { return "config_dev"; }
}
