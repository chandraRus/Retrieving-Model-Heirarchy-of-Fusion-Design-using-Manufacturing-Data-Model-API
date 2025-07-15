# Sample to Retrieve Model Hierarchy of Fusion Design using Manufacturing Data Model API

![Platforms](https://img.shields.io/badge/platform-windows%20%7C%20osx%20%7C%20linux-lightgray.svg)
[![License](http://img.shields.io/:license-mit-blue.svg)](http://opensource.org/licenses/MIT)


**APS API:** [![oAuth2](https://img.shields.io/badge/oAuth2-v2-green.svg)](https://aps.autodesk.com/en/docs/oauth/v2/developers_guide/overview/)
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

### 4. ▶️ Running the Application

Use the following command to run the application:

```bash
dotnet run
```

---

### 5. 🌐 Accessing the Web Application

Once running, open your browser and navigate to:

```
http://localhost:3000
```

You will be prompted to **log in with your Autodesk account** to authorize the app.

---

## 📊 Output

![Demo for Model Heirarchy web app](https://github.com/user-attachments/assets/ad82d421-55fe-4a06-b8fc-dd14f40c5811)


## 🔍 Workflow Explanation

The application follows these steps to help you explore your Fusion design data efficiently:

### 1️⃣ Authenticate Securely

The application uses **OAuth 3.0 for secure authentication**, requiring you to sign in with your email credentials.  
Once authenticated, you will be redirected back to the application with your session active.

### 2️⃣ Navigate Your Team Hub

At the top level, you will see your **Team Hub**, which is your team’s collaborative workspace in Fusion.

> **Example:**  
> `Team hub - Chandra Shekar Gopal`

### 3️⃣ Access Projects Inside Your Team Hub

Each **project under your Team Hub organizes related design data**.

**Examples of projects:**
- Admin Project
- Assets
- Default Project
- Manufacturing Data Model Samples

### 4️⃣ Explore Design Files Within Projects/Folders

Inside each project, you will see **design files you can select to explore in detail.**

✅ **What `DesignItem` means:**  
Indicates **Fusion design files** you can view and explore in detail, including structure and subcomponents.

**Examples:**

Under **Admin Project**:
  - `DesignItem: Utility Knife`
  - `DesignItem: Joints Demo Block`

Under **Manufacturing Data Model**:
  - `DesignItem: Utility Knife`
  - `DesignItem: Sheet Metal Example`
  - `DesignItem: 2.5D Milling - Mounting Plate`
  - `DesignItem: Tutorial1`

✅ **What `BasicItem` means:**  
Indicates **design files from other Autodesk products** (Inventor, Revit, Navisworks, DWF) you can view and analyze for structure.

**Examples:** 

Under **Default Project:**
  - `BasicItem: V8 Engine.iam` (Inventor)
  - `BasicItem: Robot.iam` (Inventor)
  - `BasicItem: Office Building.nwc` (Navisworks)
  - `BasicItem: Sports Car.dwfx` (DWF)
  - `BasicItem: House Design.rvt` (Revit)

### 5️⃣ View Details in the Right Panel

Click on any design item to view its **hierarchy, structure, and subcomponents** in the right panel.

### 6️⃣ Retrieve and Display Model Hierarchy

After clicking:
- Retrieves the **root component and references**.
- **Recursively gathers child components**.
- Uses **GraphQL queries** to fetch and structure the complete model hierarchy for analysis.

### ✅ This workflow ensures secure authentication, intuitive navigation, and deep insight into your Fusion designs.   
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
 
- **Keep your ClientId and ClientSecret secure**.
- Always use **valid credentials** when running the application.

---

## 🌟 Key Highlights

1. **Setup and Configuration**:
   - Install dependencies
   - Configure OAuth credentials    

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
