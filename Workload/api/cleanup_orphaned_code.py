import re
import os

def clean_controller(file_path):
    print(f"Cleaning {file_path}...")
    
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Remove orphaned feature flag blocks like:
    # // Check feature flag
    # {
    #     return await CreateRedirectResponse(...);
    # }
    pattern1 = r'\s*//\s*Check feature flag.*?\n\s*\{\s*\n\s*return await CreateRedirectResponse\([^;]+;\s*\n\s*\}\s*'
    content = re.sub(pattern1, '\n', content, flags=re.MULTILINE | re.DOTALL)
    
    # Remove orphaned blocks like:
    # // Check feature flag for new content API  
    # {
    #     _logger.LogInformation(...);
    #     var errorResponse = ...
    #     return notImplementedResponse;
    # }
    pattern2 = r'\s*//\s*Check feature flag for new.*?\n\s*\{\s*\n.*?return [^;]+;\s*\n\s*\}\s*'
    content = re.sub(pattern2, '\n', content, flags=re.MULTILINE | re.DOTALL)
    
    # Remove CreateRedirectResponse methods
    pattern3 = r'\s*private async Task<HttpResponseData> CreateRedirectResponse.*?^\s*\}\s*$'
    content = re.sub(pattern3, '', content, flags=re.MULTILINE | re.DOTALL)
    
    with open(file_path, 'w') as f:
        f.write(content)
    
    print(f"Cleaned {file_path}")

# Clean all controller files
controllers = [
    'Controllers/AnalyticsController.cs',
    'Controllers/ContentController.cs', 
    'Controllers/PayPalController.cs',
    'Controllers/WorkloadController.cs'
]

for controller in controllers:
    if os.path.exists(controller):
        clean_controller(controller)

print("Cleanup complete!")
