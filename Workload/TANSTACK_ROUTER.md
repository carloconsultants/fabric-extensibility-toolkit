# TanStack Router Setup

This project uses [TanStack Router](https://tanstack.com/router) with file-based routing for a type-safe, modern routing solution.

## 📁 File-Based Routing Structure

Routes are automatically generated from files in the `routes/` directory:

```
routes/
├── __root.tsx                              # Root layout with context
├── index.tsx                               # Home page (/)
├── HelloWorldItem-editor.$itemObjectId.tsx # /HelloWorldItem-editor/:itemObjectId
├── HelloWorldItem-settings-page.tsx        # /HelloWorldItem-settings-page
├── client-sdk-playground.tsx               # /client-sdk-playground
└── data-playground.tsx                     # /data-playground
```

## 🎯 Key Features

- **File-Based Routing**: Routes are automatically generated from files in `routes/`
- **Type Safety**: Full TypeScript support with type-safe navigation and params
- **Context API**: `workloadClient` is available in all routes via context
- **Auto-Generated Route Tree**: `routeTree.gen.ts` is generated automatically (gitignored)
- **Dev Tools**: Router devtools available in development mode

## 📝 Creating New Routes

### Basic Route

Create a new file in `routes/`:

```tsx
// routes/my-page.tsx
import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/my-page')({
  component: MyPage,
});

function MyPage() {
  const { workloadClient } = Route.useRouteContext();
  return <div>My Page</div>;
}
```

### Route with Parameters

Use `$paramName` in the filename:

```tsx
// routes/item.$itemId.tsx
import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/item/$itemId')({
  component: ItemPage,
});

function ItemPage() {
  const { itemId } = Route.useParams();
  const { workloadClient } = Route.useRouteContext();
  return <div>Item: {itemId}</div>;
}
```

### Nested Routes

Create subdirectories for nested routes:

```
routes/
├── dashboard/
│   ├── index.tsx          # /dashboard
│   └── settings.tsx       # /dashboard/settings
```

## 🔧 Configuration

The router is configured in `vite.config.mts`:

```typescript
TanStackRouterVite({
  routesDirectory: path.resolve(appRoot, 'routes'),
  generatedRouteTree: path.resolve(appRoot, 'routeTree.gen.ts'),
})
```

## 🚀 Navigation

Navigate programmatically using the router:

```tsx
import { useNavigate } from '@tanstack/react-router';

function MyComponent() {
  const navigate = useNavigate();
  
  const handleClick = () => {
    navigate({ to: '/my-page' });
  };
  
  return <button onClick={handleClick}>Go to My Page</button>;
}
```

## 🛠️ Development

The route tree is automatically regenerated when:
- You add/remove/rename files in `routes/`
- You run `npm run start`

## 📚 Resources

- [TanStack Router Docs](https://tanstack.com/router/latest)
- [File-Based Routing Guide](https://tanstack.com/router/latest/docs/framework/react/guide/file-based-routing)
- [Type Safety Guide](https://tanstack.com/router/latest/docs/framework/react/guide/type-safety)
