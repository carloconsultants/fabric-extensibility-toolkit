# Copilot Instructions for Frontend Development

## Overview
This frontend uses React with TypeScript, TanStack Query for data fetching, and cs-ui-library for UI components. Follow these patterns for consistent development.

## Key Technologies
- **React 18** with TypeScript
- **TanStack Query** for server state management
- **cs-ui-library** for UI components
- **Vite** for build tooling
- **Fluent UI** for additional components

## Directory Structure
- **hooks/queries/**: TanStack Query hooks for data fetching
- **clients/**: API client implementations
- **components/**: Reusable UI components
- **items/**: Fabric item editors and related components
- **playground/**: Development and testing components

## Key Patterns

### 1. TanStack Query Hooks
Create custom hooks for data fetching:
```typescript
export function useGetData(id: string) {
  return useQuery({
    queryKey: ['data', id],
    queryFn: async () => {
      const response = await apiClient.getData(id);
      return response.data;
    },
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}
```

### 2. Component Structure
Use functional components with TypeScript:
```typescript
interface MyComponentProps {
  data: MyDataType;
  onAction: (id: string) => void;
}

export function MyComponent({ data, onAction }: MyComponentProps) {
  const { data: queryData, isLoading, error } = useGetData(data.id);
  
  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;
  
  return (
    <div>
      {/* Component JSX */}
    </div>
  );
}
```

### 3. Layout Component
Always wrap your main content with the Layout component from cs-ui-library:
```typescript
import { Layout } from '@carloconsultants/cs-ui-library';

export function MyPage() {
  return (
    <Layout>
      {/* Your content */}
    </Layout>
  );
}
```

### 4. Fabric Client Usage
Use the Fabric client for Microsoft Fabric operations:
```typescript
import { getFabricClient } from '../hooks/queries/fabricQueries';

const fabricClient = getFabricClient();
const workspaces = await fabricClient.workspaces.getAllWorkspaces();
```

### 5. Error Handling
Handle errors gracefully in components:
```typescript
const { data, isLoading, error, refetch } = useQuery({
  queryKey: ['data'],
  queryFn: fetchData,
  retry: 3,
  onError: (error) => {
    console.error('Query failed:', error);
    // Handle error (show notification, etc.)
  }
});
```

## Available Hooks
- `useGetWorkspaces()`: Get all accessible workspaces
- `useGetLakehouses(workspaceId)`: Get lakehouses for a workspace
- `useGetLakehouseTables(workspaceId, lakehouseId)`: Get tables in a lakehouse
- `useFabricWorkspaceUsers()`: Get users from all workspaces
- `useFabricWorkspaceUsersByWorkspace(workspaceId)`: Get users for specific workspace

## Styling
- Use Fluent UI components when possible
- Use cs-ui-library components for custom functionality
- Follow the existing theme patterns
- Use CSS modules or styled-components for custom styling

## State Management
- Use TanStack Query for server state
- Use React state for local component state
- Use Redux only when necessary for complex client state

## Environment Variables
Access environment variables through `process.env`:
```typescript
const workloadName = process.env.WORKLOAD_NAME;
const apiUrl = process.env.BACKEND_URL;
```

## Testing
- Write unit tests for components
- Test query hooks with mock data
- Use React Testing Library for component testing
- Mock external dependencies

## Performance
- Use React.memo for expensive components
- Implement proper loading states
- Use TanStack Query's caching effectively
- Lazy load components when appropriate
