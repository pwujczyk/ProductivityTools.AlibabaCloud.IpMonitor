properties([pipelineTriggers([githubPush()])])

pipeline {
    agent any

    stages {
        stage('hello1') {
            steps {
                echo 'hello'
            }
        }
		stage('UnInstallService') {
            steps {
                powershell('If (Get-Service ProductivityTools.Alibaba -ErrorAction SilentlyContinue) {UnInstall-Service -ExePath C:\\Bin\\ProductivityTools.AlibabaCloud.IpMonitor\\ProductivityTools.AlibabaCloud.IpMonitor.exe}') 
			}
        }

        stage('UnInstallServiceNetCore') {
            steps {
                powershell('If (Get-Service PT.Alibaba -ErrorAction SilentlyContinue) {sc.exe delete PT.Alibaba}') 
			}
        }
        	
		stage('deleteWorkspace') {
            steps {
                deleteDir()
            }
        }
		
        stage('clone') {
            steps {
                git branch: 'main',
                url: 'https://github.com/pwujczyk/ProductivityTools.AlibabaCloud.IpMonitor'
            }
        }
		
		
       stage('build') {
            steps {
				echo 'starting bddduild'
                bat('dotnet publish ProductivityTools.AlibabaCloud.sln -c Release')
            }
        }
		
		stage('RemoveDirectory') {
            steps {
                powershell('Remove-Item -Recurse -Force "C:\\Bin\\Services\\ProductivityTools.AlibabaCloud\\"')
            }
        }	



		stage('CopyFiles') {
            steps {
                bat('xcopy ".\\ProductivityTools.AlibabaCloud.NetCoreService\\bin\\Release\\net9.0\\publish\\" "C:\\Bin\\Services\\ProductivityTools.AlibabaCloud.NetCoreService\\" /O /X /E /H /K /Y')
            }
        }	
		stage('InstallService') {
            steps {
                powershell('sc.exe create PT.Alibaba binpath="C:\\Bin\\Services\\ProductivityTools.AlibabaCloud.NetCoreService\\ProductivityTools.AlibabaCloud.NetCoreService.exe"')
            }
        }		
		stage('StartService') {
            steps {
                powershell('Start-Service PT.Alibaba')
            }
        }			
        stage('byebye'){
			steps {
                echo 'byebye'
            }
        }
    }
	post {
		always {
            emailext body: "${currentBuild.currentResult}: Job ${env.JOB_NAME} build ${env.BUILD_NUMBER}\n More info at: ${env.BUILD_URL}",
                recipientProviders: [[$class: 'DevelopersRecipientProvider'], [$class: 'RequesterRecipientProvider']],
                subject: "Jenkins Build ${currentBuild.currentResult}: Job ${env.JOB_NAME}"
		}
	}
}
