import { readFile } from 'fs/promises'
import { ConnectionOptions } from '@temporalio/client'

export const namespace = process.env.TEMPORAL_NAMESPACE || 'default'

export async function getConnectionOptions(): Promise<ConnectionOptions> {
  const { TEMPORAL_ADDRESS = 'localhost:7233' } = process.env

  const { TEMPORAL_TLS_CERT, TEMPORAL_TLS_KEY } = process.env
  if (TEMPORAL_TLS_CERT && TEMPORAL_TLS_KEY) {
    return {
      address: TEMPORAL_ADDRESS,
      tls: {
        clientCertPair: {
          crt: await readFile(TEMPORAL_TLS_CERT),
          key: await readFile(TEMPORAL_TLS_KEY),
        },
      },
    }
  }

  return {
    address: TEMPORAL_ADDRESS,
  }
}
