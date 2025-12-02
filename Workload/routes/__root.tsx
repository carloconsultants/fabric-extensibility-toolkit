import { createRootRouteWithContext, Outlet } from '@tanstack/react-router';
import { TanStackRouterDevtools } from '@tanstack/router-devtools';
import React from 'react';
import { WorkloadClientAPI } from '@ms-fabric/workload-client';

interface RouterContext {
  workloadClient: WorkloadClientAPI;
}

export const Route = createRootRouteWithContext<RouterContext>()({
  component: RootComponent,
});

function RootComponent() {
  return (
    <>
      <Outlet />
      {process.env.NODE_ENV === 'development' && <TanStackRouterDevtools position="bottom-right" />}
    </>
  );
}
