import { inferAsyncReturnType } from '@trpc/server';
import { CreateNextContextOptions } from '@trpc/server/adapters/next';
/**
 * Creates context for an incoming request
 * @link https://trpc.io/docs/context
 */
export declare function createContext(_opts: CreateNextContextOptions): Promise<{
    temporal: import("@temporalio/client").Client;
}>;
export type Context = inferAsyncReturnType<typeof createContext>;
//# sourceMappingURL=trpc-context.d.ts.map