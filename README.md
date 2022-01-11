# MyInfo Connector for .Net

MyInfo Connector aims to simplify consumer's integration effort with MyInfo by providing an easy to use .NET library to integrate into your application.

## Requirements

.NET 4.8

### 1.1 NuGet Package Installation

Add the following nuget packages to your application.

```xml
jose-jwt
Newtonsoft.Json
```

### 1.2 Import Connector

Add the namespace as below to access the MyInfoConnector into your code:

```.Net
namespace sg.gov.ndi;
```

### 1.3 Configuration file

Config can be supplied in a number of ways, app.config, NameValueCollection or by passing a `Func<string,string>`.

You are required to create a config file with the following key values for this library. Sample config files can be found in this repository under the Sample Configuration folder.

| Required config values | Description |
| -------- | ----------- |
| PrivateCertificateFilename | Path of the .p12 file that holds the private key. Absolute or relative path to assembly |
| PrivateCertificatePassword | Password of your private key p12 file. |
| PublicCertificateFilename | Path to the MyInfo public certificate. Absolute or relative path to assembly  |
| ClientAppId | Unique ID provided upon approval of your application to use MyInfo. For our sample application, it is **STG2-MYINFO-SELF-TEST** |
| ClientAppPassword | Secret key provided upon approval of your application to use MyInfo. For our sample application, it is **44d953c796cccebcec9bdc826852857ab412fbe2** |
| RedirectUrl | The callback URL specified when invoking the authorise call. For our sample application, it is http://localhost:3001/callback |
| AttributeCsv | Comma separated list of attributes requested. Possible attributes are listed in the Person object definition in the API specifications. |
| Environment | The environment your application is configured. This can be **SANDBOX**, **TEST** or **PROD**. |
| AuthoriseUrl | Specify the AUTHORISE API URL for MyInfo. The API is available in three environments:<br> SANDBOX: **https://sandbox.api.myinfo.gov.sg/com/v3/authorise**<br> TEST: **https://test.api.myinfo.gov.sg/com/v3/authorise**<br> PROD:  **https://api.myinfo.gov.sg/com/v3/authorise** |
| TokenUrl | Specify the TOKEN API URL for MyInfo. The API is available in three environments:<br> SANDBOX: **https://sandbox.api.myinfo.gov.sg/com/v3/token**<br> TEST: **https://test.api.myinfo.gov.sg/com/v3/token**<br> PROD:  **https://api.myinfo.gov.sg/com/v3/token** |
| PersonUrl | Specify the PERSON API URL for MyInfo. The API is available in three environments:<br> SANDBOX: **https://sandbox.api.myinfo.gov.sg/com/v3/person**<br> TEST: **https://test.api.myinfo.gov.sg/com/v3/person**<br> PROD:  **https://api.myinfo.gov.sg/com/v3/person** |
| Purpose | The text passed to the OAuth to inform the user what 

## How to use the connector

### 1. Get a single instance of MyInfoConnector

Get a single instance of MyInfoConnector and load config file:

```
MyInfoConnector connector = MyInfoConnector.getInstance("C:\\MyInfoConnectorPROD.config");
```

Once the config file is loaded, you may retrieve the instance again with the below method:
```
MyInfoConnector connector = MyInfoConnector.getCurrentInstance();
```

### 2. Retrieve person's data
Retrieve person's data by passing the authorisation code and state from the Authorise API call:

```
connector.getMyInfoPersonData(authCode,state);
```
**txnNo** is an optional parameter that can be passed through the overloaded method, if required.
```
connector.getMyInfoPersonData(authCode,txnNo,state);
```

## Helper methods

Under the hood, MyInfoConnector make use of **MyInfoSecurityHelper** and you may use the class as util methods to meet your application needs.

### 1. Forming the Signature Base String
This method takes in the API call method (GET, POST, etc.), API URL, and all the required parameters into a treemap, sort them and form the base string.
```
MyInfoSecurityHelper.GenerateBaseString(method, url, baseParams);
```

### 2. Generating the Signature
This method takes in the base string and the private key to sign and generate the signature.
```
MyInfoSecurityHelper.generateSignature(baseString, privateKey);
```

### 3. Assembling the Header
This method takes in all the required parameters into a treemap and assemble the header.
```
MyInfoSecurityHelper.GenerateAuthorizationHeader(authHeaderParams);
```
It also provide an overloaded method that takes in the bearer token, if required.
```
MyInfoSecurityHelper.generateAuthorizationHeader(authHeaderParams, bearer);
```
### 4. Verify Token
This method takes in the decrypted payload and the public key to verify the token.
```
MyInfoSecurityHelper.verifyToken(decryptedToken, pubKey);
```

## Reporting issues

You may contact [support@myinfo.gov.sg](mailto:support@myinfo.gov.sg) for any other technical issues, and we will respond to you within 5 working days.
