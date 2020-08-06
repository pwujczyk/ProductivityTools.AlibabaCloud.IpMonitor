properties([pipelineTriggers([githubPush()])])

pipeline {
    agent any

    stages {
        stage('hello') {
            steps {
                echo 'hello'
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
                bat(script: "C:\\bin\\nuget.exe restore", returnStdout: true)
            }
        }
		
		stage('build') {
            steps {
                bat(script: """ "C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\MsBuild.exe" .\\ProductivityTools.AlibabaCloud.IpMonitor.sln""", returnStdout: true)
            }
        }
		stage('InstallPSModule') {
            steps {
                powershell('Install-Module -Name ProductivityTools.PSInstallService')
            }
        }	
		stage('UnInstallService') {
            steps {
                powershell('UnInstall-Service -ServiceExePath C:\\Bin\\ProductivityTools.AlibabaCloud.IpMonitor\\ProductivityTools.AlibabaCloud.IpMonitor.exe')
            }
        }	
		stage('CopyFiles') {
            steps {
                bat('xcopy ".\\ProductivityTools.AlibabaCloud.IpMonitor.WindowsService\\bin\\Debug" "C:\\Bin\\ProductivityTools.AlibabaCloud.IpMonitor\\" /O /X /E /H /K /Y')
            }
        }	
		stage('InstallService') {
            steps {
                powershell('Install-Service -ServiceExePath C:\\Bin\\ProductivityTools.AlibabaCloud.IpMonitor\\ProductivityTools.AlibabaCloud.IpMonitor.exe')
            }
        }			
        stage('byebye'){
			steps {
                echo 'byebye'
            }
        }
    }
}