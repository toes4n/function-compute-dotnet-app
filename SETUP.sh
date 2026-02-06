#!/bin/bash
# Complete setup and deployment guide for DevOps Learning Application

cat << 'EOF'

╔═══════════════════════════════════════════════════════════════════════╗
║                                                                       ║
║     DevOps Learning Application - Complete Setup Guide              ║
║     .NET 8.0 for Alibaba Function Compute                           ║
║                                                                       ║
╚═══════════════════════════════════════════════════════════════════════╝

📦 ALL FILES NEEDED:
═══════════════════════════════════════════════════════════════════════

1. devops-app.csproj      - Project configuration
2. Program.cs             - Main application code
3. bootstrap              - Function Compute entry script
4. Makefile               - Build automation (optional)
5. .gitignore             - Git configuration (optional)
6. README.md              - Complete documentation


🚀 SETUP INSTRUCTIONS:
═══════════════════════════════════════════════════════════════════════

STEP 1: Create Clean Directory
────────────────────────────────────────────────────────────────────────
$ mkdir devops-app-final
$ cd devops-app-final

STEP 2: Add All 6 Files to This Directory
────────────────────────────────────────────────────────────────────────
Copy these files into devops-app-final/:
  ✓ devops-app.csproj
  ✓ Program.cs
  ✓ bootstrap
  ✓ Makefile
  ✓ .gitignore
  ✓ README.md


🔨 BUILD PROCESS:
═══════════════════════════════════════════════════════════════════════

STEP 3: Build Application
────────────────────────────────────────────────────────────────────────
$ dotnet build -c Release

Expected output: Build succeeded ✓


STEP 4: Publish for Linux x64
────────────────────────────────────────────────────────────────────────
$ dotnet publish -c Release -r linux-x64 -o ./publish

This creates a self-contained binary for Linux.
Output directory: ./publish/


STEP 5: Create Deployment Package
────────────────────────────────────────────────────────────────────────
$ cd publish
$ chmod +x bootstrap
$ cd ..
$ zip -r function.zip publish/bootstrap publish/devops-app \
    publish/*.dll publish/*.json publish/*.runtimeconfig.json

Result: function.zip (ready for upload)


🌐 LOCAL TESTING (Optional):
═══════════════════════════════════════════════════════════════════════

$ dotnet run --project devops-app.csproj

In another terminal:
$ curl http://localhost:9000/health | jq .
$ curl http://localhost:9000/config | jq .
$ curl http://localhost:9000/metrics | jq .


📤 DEPLOY TO ALIBABA FUNCTION COMPUTE:
═══════════════════════════════════════════════════════════════════════

STEP 6: Create Service (one time)
────────────────────────────────────────────────────────────────────────
$ fc3 service create \
  --service-name devops-learning \
  --description "DevOps Learning Application"


STEP 7: Create Function
────────────────────────────────────────────────────────────────────────
$ fc3 function create \
  --service-name devops-learning \
  --function-name learning-app \
  --handler index.handler \
  --runtime custom \
  --zip-file fileb://./function.zip \
  --memory-size 512 \
  --timeout 60


STEP 8: Create HTTP Trigger
────────────────────────────────────────────────────────────────────────
$ fc3 trigger create \
  --service-name devops-learning \
  --function-name learning-app \
  --trigger-name http-trigger \
  --trigger-type http


STEP 9: Set Environment Variables
────────────────────────────────────────────────────────────────────────
$ fc3 function update \
  --service-name devops-learning \
  --function-name learning-app \
  --env-vars 'ENVIRONMENT=production,LOG_LEVEL=Information'


🧪 TEST YOUR DEPLOYMENT:
═══════════════════════════════════════════════════════════════════════

Get your function URL from Alibaba Console, then:

$ curl https://<your-function-url>/health | jq .
$ curl https://<your-function-url>/config | jq .
$ curl https://<your-function-url>/metrics | jq .
$ curl https://<your-function-url>/deploy | jq .


⚙️  USING MAKEFILE (SHORTCUT):
═══════════════════════════════════════════════════════════════════════

If you have Makefile in your directory:

$ make build              # Build application
$ make publish            # Publish for Linux
$ make package            # Create ZIP
$ make clean              # Remove build artifacts
$ make deploy             # Deploy to Function Compute


📋 COMMON ERRORS & FIXES:
═══════════════════════════════════════════════════════════════════════

ERROR: "CS0101: The namespace already contains a definition"
FIX:   Delete any duplicate Program.cs files
       rm -f Program.cs.bak Program.old.cs

ERROR: "Unable to find package"
FIX:   Verify .csproj has correct packages:
       - Microsoft.Extensions.Logging (8.0.0)
       - Microsoft.Extensions.Logging.Console (8.0.0)

ERROR: "Function timeout"
FIX:   Increase timeout in Alibaba Console (max 600 seconds)

ERROR: "Out of memory"
FIX:   Allocate more memory (512MB minimum, try 1GB)


✅ VERIFICATION CHECKLIST:
═══════════════════════════════════════════════════════════════════════

Before deploying, verify:

☑ All 6 files copied to project directory
☑ No duplicate Program.cs files
☑ Bootstrap script is executable (chmod +x bootstrap)
☑ .csproj has correct target framework (net8.0)
☑ Dependencies are correct (Logging packages only)
☑ Built successfully with "dotnet build"
☑ Published to ./publish/ directory
☑ function.zip created successfully
☑ Bootstrap inside ZIP is executable


📚 ENDPOINTS AVAILABLE:
═══════════════════════════════════════════════════════════════════════

GET /                 - Service overview
GET /health           - Health check (for load balancers)
GET /metrics          - Performance metrics
GET /config           - Configuration & environment variables
GET /deploy           - Deployment best practices
GET /info             - System & runtime information
GET /logs             - Logging integration guide


🔐 ENVIRONMENT VARIABLES TO SET:
═══════════════════════════════════════════════════════════════════════

ENVIRONMENT=production     (or: development, staging)
LOG_LEVEL=Information      (or: Debug, Warning, Error)


🎓 LEARNING OUTCOMES:
═══════════════════════════════════════════════════════════════════════

This project teaches:
✓ Serverless architecture & Function Compute
✓ Health checks for orchestration
✓ Metrics & monitoring
✓ Environment-based configuration
✓ HTTP server implementation
✓ Error handling & logging
✓ DevOps best practices
✓ CI/CD deployment patterns


📖 DOCUMENTATION:
═══════════════════════════════════════════════════════════════════════

See README.md for detailed information:
- Architecture overview
- Troubleshooting guide
- Advanced configurations
- CI/CD integration examples


❓ HELP COMMANDS:
═══════════════════════════════════════════════════════════════════════

$ dotnet --version           # Check .NET version
$ dotnet --help              # .NET CLI help
$ fc3 --help                 # Function Compute CLI help
$ curl -V                    # Check curl version


═══════════════════════════════════════════════════════════════════════

Ready to deploy? Start with STEP 1 above!

Happy DevOps learning! 🚀

═══════════════════════════════════════════════════════════════════════

EOF
