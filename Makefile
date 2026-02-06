.PHONY: build publish package clean deploy test help

APP_NAME=devops-app
SERVICE_NAME?=devops-learning
FUNCTION_NAME?=learning-app
ENVIRONMENT?=production

build:
	@echo "🔨 Building application..."
	dotnet build -c Release

publish:
	@echo "📦 Publishing for Linux x64..."
	rm -rf publish/
	dotnet publish -c Release -r linux-x64 -o ./publish

package: publish
	@echo "📦 Creating deployment package..."
	cd publish && chmod +x bootstrap && cd ..
	rm -f function.zip
	zip -r function.zip publish/bootstrap publish/$(APP_NAME) \
		publish/*.dll publish/*.json publish/*.runtimeconfig.json -q
	@echo "✓ Package created: function.zip"
	@ls -lh function.zip

clean:
	@echo "🧹 Cleaning build artifacts..."
	rm -rf publish/ bin/ obj/ *.zip

deploy: package
	@echo "🚀 Deploying to Function Compute..."
	fc3 function update \
		--service-name "$(SERVICE_NAME)" \
		--function-name "$(FUNCTION_NAME)" \
		--zip-file "fileb://./function.zip" \
		--env-vars "ENVIRONMENT=$(ENVIRONMENT),LOG_LEVEL=Information"
	@echo "✓ Deployment complete!"

test:
	@echo "🧪 Testing endpoints..."
	@echo "Note: Update <function-url> with your actual Function Compute URL"
	@echo ""
	@echo "Health check:"
	@echo "  curl https://<function-url>/health"
	@echo ""
	@echo "Configuration:"
	@echo "  curl https://<function-url>/config"
	@echo ""
	@echo "Metrics:"
	@echo "  curl https://<function-url>/metrics"
	@echo ""
	@echo "Deployment Guide:"
	@echo "  curl https://<function-url>/deploy"

help:
	@echo "Available targets:"
	@echo "  make build       - Build the application"
	@echo "  make publish     - Publish for Linux x64"
	@echo "  make package     - Create deployment ZIP"
	@echo "  make clean       - Remove build artifacts"
	@echo "  make deploy      - Deploy to Function Compute"
	@echo "  make test        - Show test commands"
	@echo ""
	@echo "Examples:"
	@echo "  make build"
	@echo "  make package"
	@echo "  make deploy SERVICE_NAME=my-service FUNCTION_NAME=my-function"
