export declare const taskQueue = "durable-delivery";
export interface Image {
    src: string;
    alt: string;
    width: number;
    height: number;
}
export interface Product {
    id: number;
    name: string;
    description: string;
    image: Image;
    cents: number;
}
export interface Order {
    id: string;
    product: Product;
    createdAt: Date;
}
export declare const products: {
    id: number;
    name: string;
    description: string;
    image: {
        src: string;
        alt: string;
        width: number;
        height: number;
    };
    cents: number;
}[];
export declare function getProductById(id: number): Product | undefined;
export declare function errorMessage(error: unknown): string | undefined;
export declare const statusColors: Record<string, string>;
//# sourceMappingURL=index.d.ts.map