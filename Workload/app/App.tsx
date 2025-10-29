import React from "react";
import { Provider } from "react-redux";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ClientSDKStore } from "./playground/ClientSDKPlayground/Store/Store";
import { Route, Router, Switch } from "react-router-dom";
import { History } from "history";
import { WorkloadClientAPI } from "@ms-fabric/workload-client";
import { Layout } from "@carloconsultants/cs-ui-library";
import CustomItemSettings from "./items/HelloWorldItem/HelloWorldItemEditorSettingsPage";
import CustomAbout from "./items/HelloWorldItem/HelloWorldItemEditorAboutPage";
import { SamplePage, ClientSDKPlayground } from "./playground/ClientSDKPlayground/ClientSDKPlayground";
import { DataPlayground } from "./playground/DataPlayground/DataPlayground";
import { HelloWorldItemEditor} from "./items/HelloWorldItem/HelloWorldItemEditor";
import { initializeFabricClient } from "./hooks/queries/fabricQueries";
import { ExampleComponent } from "./components/ExampleComponent";

/*
    Add your Item Editor in the Route section of the App function below
*/

interface AppProps {
    history: History;
    workloadClient: WorkloadClientAPI;
}

export interface PageProps {
    workloadClient: WorkloadClientAPI;
    history?: History
}

export interface ContextProps {
    itemObjectId?: string;
    workspaceObjectId?: string
    source?: string;
}

export interface SharedState {
    message: string;
}

// Create a QueryClient instance
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      cacheTime: 10 * 60 * 1000, // 10 minutes
      retry: 3,
      refetchOnWindowFocus: false,
    },
  },
});

export function App({ history, workloadClient }: AppProps) {
    console.log('🎯 App component rendering with history:', history);
    console.log('🎯 Current location:', history.location);

    // Initialize Fabric client
    React.useEffect(() => {
        initializeFabricClient(workloadClient);
    }, [workloadClient]);

    return (
        <QueryClientProvider client={queryClient}>
            <Layout>
                <Router history={history}>
                    {/* Test route for debugging */}
                    <Route exact path="/">
                        <div style={{ padding: '20px', backgroundColor: '#f0f0f0' }}>
                            <h1>🎉 Workload is running!</h1>
                            <p>Current URL: {window.location.href}</p>
                            <p>Workload Name: {process.env.WORKLOAD_NAME}</p>
                        </div>
                    </Route>
                    
                    {/* Example component route */}
                    <Route path="/example">
                        <ExampleComponent />
                    </Route>    
                    <Switch>
                        {/* Routings for the Hello World Item Editor */}
                        <Route path="/HelloWorldItem-editor/:itemObjectId">
                            <HelloWorldItemEditor
                                workloadClient={workloadClient} data-testid="HelloWorldItem-editor" />
                        </Route>
                        
                        <Route path="/HelloWorldItem-settings-page/:itemObjectId">
                            <CustomItemSettings 
                                workloadClient={workloadClient}
                                    data-testid="HelloWorldItem-settings-page" />
                        </Route>
                        <Route path="/HelloWorldItem-about-page/:itemObjectId">
                            <CustomAbout  workloadClient={workloadClient} 
                                data-testid="HelloWorldItem-about-page" />
                        </Route>

                         {/* Playground routes  can be deleted if not needed */}
                        <Route path="/client-sdk-playground">
                            <Provider store={ClientSDKStore}>
                                <ClientSDKPlayground workloadClient={workloadClient} />
                            </Provider>
                        </Route>
                        <Route path="/data-playground">
                            <DataPlayground workloadClient={workloadClient} />
                        </Route>

                        <Route path="/sample-page">
                            <SamplePage workloadClient={workloadClient} />
                        </Route>
                    </Switch>
                </Router>
            </Layout>
        </QueryClientProvider>
    );
}