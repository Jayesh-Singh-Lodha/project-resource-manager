# End-to-End Deployment Guide (100% Free)

This document outlines the detailed steps to deploy the Project Resource Manager (PRM) application to cloud hosting services using completely free tiers. 

We will use **Render.com** for the backend API (with an embedded SQLite database) and **Vercel** for the React frontend.

---

## Code Preparation (Already Completed)

The codebase has been automatically configured for this free deployment approach:
1. **Database Swapped**: The application has been switched from Microsoft SQL Server to **SQLite**. The database is now a local file (`prm.db`), meaning you do not need to pay for or configure external database hosting.
2. **Docker Ready**: A `Dockerfile` was added to the root directory, allowing the API to be deployed seamlessly as a container on Render.com.
3. **CORS Configuration**: Updated `Program.cs` to read allowed origins from the configuration so the frontend can securely call the API.

---

## Deployment Steps (Manual Execution)

Follow these steps sequentially to get the application live.

### Phase 1: Backend API Deployment (Render.com)
1. Ensure your entire repository is pushed to a **GitHub** repository.
2. Go to [Render.com](https://render.com/) and create a free account.
3. On the Dashboard, click **New +** and select **Web Service**.
4. Click **Build and deploy from a Git repository** and connect your GitHub account.
5. Select the repository containing the PRM code.
6. In the configuration settings:
   - **Name:** e.g., `prm-api`
   - **Environment:** Render should automatically detect `Docker` as the environment because of the `Dockerfile`.
   - **Instance Type:** Select the **Free** tier.
7. Scroll down to **Advanced** and expand the **Environment Variables** section. Add the following:
   - `JwtSettings__SecretKey`: (Generate a long, secure random string, e.g., `MySuperSecretKeyForPrm123456789!`)
   - `JwtSettings__Issuer`: `PRM_API_Production`
   - `JwtSettings__Audience`: `PRM_Users`
   - `LlmSettings__ApiKey`: (Your Gemini or Groq API Key)
   - `AllowedOrigins`: (Leave this blank for now, we will come back and fill it in Phase 3)
8. Click **Create Web Service**.
9. Render will begin building the Docker image and deploying it. Once it's live, copy the URL at the top left of the dashboard (e.g., `https://prm-api.onrender.com`).

*(Note: On Render's Free tier, the SQLite database data will be reset whenever the service sleeps from inactivity or is redeployed. This is acceptable for a personal portfolio project. If you need persistent data in the future, you would upgrade to a paid Render plan with a "Disk" attached).*

### Phase 2: Frontend Deployment (Vercel)
1. Go to [Vercel](https://vercel.com/) and create a free account.
2. Click **Add New... > Project**.
3. Import your GitHub repository.
4. In the configuration step:
   - Set the **Framework Preset** to **Vite**.
   - Set the **Root Directory** to `frontend/` (Important!).
5. Expand **Environment Variables** and add:
   - `VITE_API_BASE_URL`: Paste the URL of your deployed Render Web Service (e.g., `https://prm-api.onrender.com`).
6. Click **Deploy**.

### Phase 3: Final Connection (CORS Update)
1. Once Vercel finishes deploying, copy your live frontend URL (e.g., `https://prm-frontend.vercel.app`).
2. Go back to your Web Service in the **Render Dashboard**.
3. Navigate to the **Environment** tab on the left menu.
4. Update the `AllowedOrigins` environment variable we left blank earlier:
   - Value: `https://prm-frontend.vercel.app` (paste your exact Vercel URL, no trailing slash).
5. Save changes. Render will restart the service to apply the new setting.

### 🎉 Success!
Your application is now deployed end-to-end completely for free. 
- The database will automatically seed the initial `admin` (password: `Admin@1234`) user on the first run of the API on Render.
- **Frontend URL:** (Your Vercel Link)
