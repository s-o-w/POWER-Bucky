// Copyright (c) Microsoft. All rights reserved.

import { Body1, Spinner, Title3 } from '@fluentui/react-components';
import { FC, useEffect } from 'react';

interface IData {
    uri: string;
    onBackendFound: () => void;
}

const BackendProbe: FC<IData> = ({ uri, onBackendFound }) => {
    useEffect(() => {
        const timer = setInterval(() => {
            const requestUrl = new URL('healthz', uri);
            const fetchAsync = async () => {
                const result = await fetch(requestUrl);

                if (result.ok) {
                    onBackendFound();
                }
            };

            fetchAsync().catch(() => {
                // Ignore - this page is just a probe, so we don't need to show any errors if backend is not found
            });
        }, 3000);

        return () => {
            clearInterval(timer);
        };
    });

    return (
        <div style={{ padding: 80, gap: 20, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            <Title3>Looking for your backend</Title3>
            <Spinner />
            <Body1>
                This sample expects to find a Semantic Kernel service from <strong>webapi/</strong> running at{' '}
                <strong>{uri}</strong>
            </Body1>
            <Body1>
                Run your Semantic Kernel service locally using Visual Studio, Visual Studio Code or by typing the
                following command: <strong>dotnet run</strong>
            </Body1>
        </div>
    );
};

export default BackendProbe;
