import React from "react";
import { RouterProvider, createRouter } from "@tanstack/react-router";
import { WorkloadClientAPI } from "@ms-fabric/workload-client";
import { routeTree } from "./routeTree.gen";

// Create the router instance
const router = createRouter({
  routeTree,
  context: {
    workloadClient: undefined!,
  },
});

// Register router for type safety
declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}

/*
    Add your Item Editor routes in the routes/ folder
    TanStack Router will automatically generate the route tree
*/

interface AppProps {
  workloadClient: WorkloadClientAPI;
}

export interface PageProps {
  workloadClient: WorkloadClientAPI;
}

export interface ContextProps {
  itemObjectId?: string;
  workspaceObjectId?: string;
  source?: string;
}

export interface SharedState {
  message: string;
}

export function App({ workloadClient }: AppProps) {
  console.log('🎯 App component rendering with TanStack Router');

  return (
    <RouterProvider
      router={router}
      context={{ workloadClient }}
    />
  );
}