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
        stage('byebye') {
                echo 'byebye'
            }
        }
    }
