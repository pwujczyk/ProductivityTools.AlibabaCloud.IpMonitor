properties([pipelineTriggers([githubPush()])])

pipeline {
    agent any

    stages {
        stage('hello') {
            steps {
                echo 'hello'
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
                bat(script: "dotnet build ProductivityTools.AlibabaCloud.IpMonitor.sln -c Release ", returnStdout: true)
            }
        }
		stage('CopyFiles') {
            steps {
                bat('xcopy ".\\ProductivityTools.AlibabaCloud.IpMonitor.WindowsService\\bin\\Release" "C:\\Bin\\ProductivityTools.AlibabaCloud.IpMonitor\\" /O /X /E /H /K')
            }
        }	
		stage('getprocess') {
            steps {
                powershell('get-process')
            }
        }				
        stage('byebye'){
			steps {
                echo 'byebye'
            }
        }
    }
}