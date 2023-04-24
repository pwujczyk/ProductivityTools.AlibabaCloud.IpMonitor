<!--Category:C#--> 
 <p align="right">
    <a href="http://productivitytools.top/alibabacloud-ipmonitor/"><img src="Images/Header/ProductivityTools_green_40px_2.png" /><a> 
    <a href="https://github.com/pwujczyk/ProductivityTools.AlibabaCloud.IpMonitor"><img src="Images/Header/Github_border_40px.png" /></a>
</p>
<p align="center">
    <a href="http://http://productivitytools.tech/">
        <img src="Images/Header/LogoTitle_green_500px.png" />
    </a>
</p>


# AlibabaCloud.IpMonitor

Windows service which keep Alibaba DNS in sync with public address of the server.

<!--more-->

I do not have public IP, but I have server where I host applications. This application is installed as a service. Every x seconds it checks what is the assigned public IP to my router and updates value on Alibaba DNS servers. 


In the config we have values
- GmailPassword - Application sends email when value it will update value or in case of error
- Hosts - those hosts will be updated in the Alibaba
```
{
  "Region": "cn-hangzhou",
  "AccessKeyId":"",
  "AccessKeySecret":"",
  "GmailPassword":"", 
  "Hosts":["purchase","echo","jenkinswebhook","meetings","identityserver","apiteammanagement","apigettask3"]
}
```

![](Images/2023-04-23-07-49-56.png)