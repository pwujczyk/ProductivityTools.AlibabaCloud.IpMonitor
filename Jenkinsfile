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
                bat(script: "C:\bin\nuget.exe restore", returnStdout: true)
            }
        }
		
		stage('build') {
            steps {
                bat(script: "dotnet build ProductivityTools.AlibabaCloud.IpMonitor.sln -c Release ", returnStdout: true)
            }
        }
		
        stage('byebye'){
			steps {
                echo 'byebye'
            }
        }
    }
}