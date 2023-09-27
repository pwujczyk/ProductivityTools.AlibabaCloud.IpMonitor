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
        	
		stage('deleteWorkspace') {
            steps {
                deleteDir()
            }
        }
		
        stage('clone') {
            steps {
                git branch: 'master',
                url: 'https://github.com/pwujczyk/ProductivityTools.AlibabaCloud.IpMonitor'
            }
        }
		
		stage('restore') {
            steps {
                bat(script: "C:\\Bin\\SimpleMarketPlace\\App.Nuget\\nuget.exe restore", returnStdout: true)
            }
        }
		
		stage('build') {
            steps {
                bat(script: """ "C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\MsBuild.exe" .\\ProductivityTools.AlibabaCloud.IpMonitor.sln""", returnStdout: true)
            }
        }
		stage('InstallPSModule') {
            steps {
                powershell('Install-Module -Name ProductivityTools.InstallService -Force')
            }
        }	
		
		stage ("60secWaiting") {
			steps{
				 powershell('start-sleep -seconds 60')
			}
		}
		
		stage('RemoveDirectory') {
            steps {
                powershell('Remove-Item -Recurse -Force "C:\\Bin\\ProductivityTools.AlibabaCloud.IpMonitor\\"')
            }
        }	



		stage('CopyFiles') {
            steps {
                bat('xcopy ".\\ProductivityTools.AlibabaCloud.IpMonitor.WindowsService\\bin\\Debug" "C:\\Bin\\ProductivityTools.AlibabaCloud.IpMonitor\\" /O /X /E /H /K /Y')
            }
        }	
		stage('InstallService') {
            steps {
                powershell('Install-Service -ExePath C:\\Bin\\ProductivityTools.AlibabaCloud.IpMonitor\\ProductivityTools.AlibabaCloud.IpMonitor.exe')
            }
        }		
		stage('StartService') {
            steps {
                powershell('Start-Service ProductivityTools.Alibaba')
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
