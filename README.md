# Sample to Retrieve Model Hierarchy of Fusion Design using Manufacturing Data Model API

![Platforms](https://img.shields.io/badge/platform-windows%20%7C%20osx%20%7C%20linux-lightgray.svg)
[![License](http://img.shields.io/:license-mit-blue.svg)](http://opensource.org/licenses/MIT)


**APS API:** [![oAuth2](https://img.shields.io/badge/oAuth2-v2-green.svg)](https://aps.autodesk.com/en/docs/oauth/v1/developers_guide/overview/)
[![Manufacturing Data Model API](https://img.shields.io/badge/Manufacturing%20Data%20Model-v2-orange)](https://aps.autodesk.com/developer/overview/manufacturing-data-model-api)
# 📚 Read the Complete Model Hierarchy of a Fusion Design

This project demonstrates how to authenticate using Autodesk's APS API, query the Manufacturing Data Model via GraphQL, and visualize the **full model hierarchy** of a design.

---

## 🚀 Setting up your Test

Follow the steps below to set up and run the project.

---

### 1. 📦 Install Necessary Dependencies

Use the **Package Manager Console** to install the required packages:

```powershell
Install-Package Microsoft.AspNetCore.Authentication.Cookies
Install-Package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
Install-Package Microsoft.AspNetCore.Session
Install-Package Microsoft.Extensions.Http
Install-Package System.Text.Json
Install-Package GraphQL.Client
Install-Package GraphQL.Client.Serializer.SystemTextJson
```

---

### 2. 🛠️ Creating an App in APS

- Create an application at the [APS Portal](https://aps.autodesk.com/).
- Retrieve your **ClientId** and **ClientSecret**.
- Set the **Callback URL** to:

```
http://localhost:3000/callback/oauth
```

Example:

![credentials](https://github.com/user-attachments/assets/b52eb760-c1ff-46ef-8651-918e6964ff8a)


> ⚠️ **Important:** The Callback URL in your APS app must match exactly.

---

### 3. 🔐 Configure OAuth Credentials

In the `appsettings.json` file, set your Autodesk Forge App credentials:

```json
{
  "OAuth": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "RedirectUri": "http://localhost:3000/callback/oauth",
    "TokenUrl": "https://developer.api.autodesk.com/authentication/v2/token",
    "Scopes": "data:read data:write data:create"
  }
}
```

> Ensure the **Callback URL** in your code matches the one registered in the APS app.

---

### 4. 📝 Set Required Variables

Set the following variables based on your design:

- `hubName`
- `projectName`
- `componentName`

in .\Controllers\ModelHierarchyController.cs file in this ModelHierarchyApp web application

You can find these in **Fusion Teams**, **Fusion 360**, or other Autodesk platforms.

Example:

![inputs](https://github.com/user-attachments/assets/2f755a61-a1dd-4257-93d9-642debf08e2e)


---

### 5. ▶️ Running the Application

Use the following command to run the application:

```bash
dotnet run
```

---

### 6. 🌐 Accessing the Web Application

Once running, open your browser and navigate to:

```
http://localhost:3000
```

You will be prompted to **log in with your Autodesk account** to authorize the app.

---

## 📊 Output

After logging in, you will see a model hierarchy similar to:

```plaintext
#Model hierarchy:

#Joints Demo Block
 
  - Planar + Tangency
  - Dual Pin Slot
  - Cylindrical
  - Base Plate
  - Rigid
  - 98541A445_External Retaining Ring
  - Ball
  - Slider
  - Pin Slot
  - Revolute
  - Planar
  - 98541A445_External Retaining Ring
```

### Example Output:
- **Root Component Name:** `Joints Demo Block`
- **Child Components:**

  - Planar + Tangency
  - Dual Pin Slot
  - Cylindrical
  - Base Plate
  - Rigid
  - 98541A445_External Retaining Ring
  - Ball
  - Slider
  - Pin Slot
  - Revolute
  - Planar
  - 98541A445_External Retaining Ring


---

## 🔍 Workflow Explanation

The application follows these steps:

1. **Retrieve** the root component and references using the provided `hubName`, `projectName`, and `componentName`.
2. **Recursively gather** child components for each parent.
3. **Use GraphQL queries** to fetch model hierarchy data.

---

## 🔗 GraphQL Query

The following **GraphQL query** is used to fetch the model hierarchy:

```graphql
query GetModelHierarchy($hubName: String!, $projectName: String!, $componentName: String!) {
  hubs(filter: { name: $hubName }) {
    results {
      name
      projects(filter: { name: $projectName }) {
        results {
          name
          items(filter: { name: $componentName }) {
            results {
              ... on DesignItem {
                name
                tipRootComponentVersion {
                  id
                  name
                  allOccurrences {
                    results {
                      parentComponentVersion {
                        id
                      }
                      componentVersion {
                        id
                        name
                      }
                    }
                    pagination {
                      cursor
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}
```

---

## 📝 Conclusion

This application provides an **easy-to-use** interface to:

- Authenticate with Autodesk APS
- Retrieve your model hierarchy
- Display the component structure in a friendly way

Once everything is set up, you can explore your **Autodesk design hierarchy** with ease!

---

## ⚠️ Notes

- Ensure `hubName`, `projectName`, and `componentName` are valid for your Autodesk account.
- Designs must **not** be nested inside folders for this sample to work.
- **Keep your ClientId and ClientSecret secure**.
- Always use **valid credentials** when running the application.

---

## 🌟 Key Highlights

1. **Setup and Configuration**:
   - Install dependencies
   - Configure OAuth credentials
   - Set required variables

2. **Running the Application**:
   - Instructions to start the application
   - Log in using Autodesk account
   - View the model hierarchy at `http://localhost:3000`

3. **Detailed Workflow**:
   - From OAuth authentication to fetching model hierarchy via GraphQL

4. **GraphQL Query**:
   - The complete query used to retrieve model data.

---

> 🎯 **Happy exploring the model hierarchy!**

---

# 📄 License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

---

# ✍️ Written by

Chandra shekar G [chandra.shekar.gopal@autodesk.com](chandra.shekar.gopal@autodesk.com), [Autodesk Partner Development](http://aps.autodesk.com)

---

Please refer to this page for more details: [Manufacturing Data Model API Docs](https://aps.autodesk.com/en/docs/mfgdataapi/v2/developers_guide/overview/) 




 
