# DevOps Learning Application for Alibaba Function Compute

Complete .NET 8.0 application with comprehensive DevOps learning features.

## 📦 Project Files

| File | Purpose |
|------|---------|
| **devops-app.csproj** | Project configuration |
| **Program.cs** | Main application code (HTTP server + endpoints) |
| **bootstrap** | Entry point for Function Compute |
| **Makefile** | Build automation |
| **.gitignore** | Git configuration |
| **README.md** | This file |

## 🚀 Quick Start

### 1. Clean Start (Recommended)
```bash
# Create fresh directory
mkdir devops-app-final
cd devops-app-final

# Copy all files into this directory:
# - devops-app.csproj
# - Program.cs
# - bootstrap
# - .gitignore
# - Makefile
# - README.md
```

### 2. Build Application
```bash
# Build for Release
dotnet build -c Release

# Or use Makefile
make build
```

### 3. Publish for Linux
```bash
# Publish as self-contained binary for Linux x64
dotnet publish -c Release -r linux-x64 -o ./publish

# Or use Makefile
make publish
```

### 4. Create Deployment Package
```bash
# Package all files into ZIP
cd publish
chmod +x bootstrap
cd ..
zip -r function.zip publish/bootstrap publish/devops-app \
    publish/*.dll publish/*.json publish/*.runtimeconfig.json

# Or use Makefile
make package
```

## 📋 HTTP Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/` | GET | Service overview |
| `/health` | GET | Health check for orchestration |
| `/metrics` | GET | Request metrics and memory usage |
| `/config` | GET | Configuration & environment variables |
| `/deploy` | GET | Deployment best practices |
| `/info` | GET | System & runtime information |
| `/logs` | GET | Logging integration guide |

## 🌐 Test Locally

```bash
# Build and run
dotnet build -c Release
dotnet run --project devops-app.csproj

# In another terminal, test endpoints:
curl http://localhost:9000/
curl http://localhost:9000/health
curl http://localhost:9000/metrics | jq .
curl http://localhost:9000/config | jq .
```

## 📤 Deploy to Alibaba Function Compute

### Prerequisites
- Alibaba Cloud account
- Function Compute service activated
- `fc3` CLI installed (`curl https://gosspublic.alicdn.com/fc-cli/release/v3/install.sh | sh`)
- Credentials configured (`fc3 config init`)

### Deployment Steps

**Step 1: Create Service (if not exists)**
```bash
fc3 service create \
  --service-name devops-learning \
  --description "DevOps Learning Application"
```

**Step 2: Create Function**
```bash
fc3 function create \
  --service-name devops-learning \
  --function-name learning-app \
  --handler index.handler \
  --runtime custom \
  --zip-file fileb://./function.zip \
  --memory-size 512 \
  --timeout 60
```

**Step 3: Create HTTP Trigger**
```bash
fc3 trigger create \
  --service-name devops-learning \
  --function-name learning-app \
  --trigger-name http-trigger \
  --trigger-type http
```

**Step 4: Set Environment Variables**
```bash
fc3 function update \
  --service-name devops-learning \
  --function-name learning-app \
  --env-vars 'ENVIRONMENT=production,LOG_LEVEL=Information'
```

### Or Use Makefile for Everything

```bash
# Build + Package + Deploy (all in one)
make clean
make package
make deploy SERVICE_NAME=devops-learning FUNCTION_NAME=learning-app ENVIRONMENT=production
```

## 🧪 Test After Deployment

Get your function URL from Alibaba Console, then:

```bash
# Health check
curl https://<your-function-url>/health | jq .

# Configuration
curl https://<your-function-url>/config | jq .

# Metrics
curl https://<your-function-url>/metrics | jq .

# Deployment guide
curl https://<your-function-url>/deploy | jq .
```

## 🏗️ Architecture

### Built-in HTTP Server
- Uses `System.Net.HttpListener`
- Listens on port 9000 (Function Compute default)
- No external dependencies

### Request Routing
- Pattern-based endpoint matching
- Comprehensive error handling
- JSON responses

### Monitoring
- Invocation metrics per endpoint
- Memory usage tracking
- Uptime calculation

## 💡 DevOps Best Practices Demonstrated

✅ **Stateless Design** - Each request independent  
✅ **Health Checks** - For load balancers and orchestrators  
✅ **Metrics & Monitoring** - Request tracking and performance  
✅ **Environment Variables** - Configuration injection  
✅ **Error Handling** - Comprehensive exception management  
✅ **Structured Logging** - Observable operations  
✅ **Cold Start Optimization** - Resource reuse  
✅ **Security** - No hardcoded secrets  

## 🔧 Configuration

### Environment Variables
```bash
ENVIRONMENT=production          # dev/staging/prod
LOG_LEVEL=Information          # Debug, Information, Warning, Error
FC_SERVER_PORT=9000            # Function Compute port (auto-set)
```

### Memory & Timeout
- **Memory**: 512 MB minimum recommended
- **Timeout**: 60 seconds (can adjust up to 600)
- **Runtime**: .NET 8.0 LTS (supported until Nov 2026)

## 📊 Project Structure

```
devops-app/
├── devops-app.csproj          # Project file
├── Program.cs                 # Main application
├── bootstrap                  # Function Compute entry script
├── Makefile                   # Build automation
├── .gitignore                 # Git configuration
├── README.md                  # This file
│
└── publish/                   # (generated after publish)
    ├── bootstrap              # Executable
    ├── devops-app             # Binary
    ├── *.dll                  # .NET libraries
    ├── *.json                 # Configuration
    └── *.runtimeconfig.json   # Runtime config
```

## 🐛 Troubleshooting

### Build Error: Package not found
**Solution**: Make sure you're using .NET 8.0 SDK
```bash
dotnet --version  # Should be 8.0.x
```

### Function Times Out
**Solution**: Increase timeout in Function Compute settings (max 600 seconds)

### Memory Errors
**Solution**: Allocate more memory (512MB minimum recommended, try 1GB for safety)

### Cold Start Latency
**Solution**: Use reserved instances to keep function warm

## 🔗 References

- [Alibaba Function Compute Documentation](https://www.alibabacloud.com/help/en/function-compute)
- [.NET 8.0 Documentation](https://learn.microsoft.com/dotnet/)
- [fc3 CLI Documentation](https://github.com/aliyun/fc3)
- [HttpListener Documentation](https://learn.microsoft.com/dotnet/api/system.net.httplistener)

## 📝 Notes

- .NET 8.0 is LTS (Long-Term Support) until November 2026
- Application is fully self-contained (no runtime dependencies)
- All code uses standard .NET libraries (no external SDK)
- Compatible with Linux x64 runtime

## 👨‍💻 Developer

Created for DevOps learning with Alibaba Function Compute.
Feel free to extend and customize!

---

**Status**: ✅ Ready for production deployment
