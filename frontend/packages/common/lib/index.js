"use strict";
exports.__esModule = true;
exports.statusColors = exports.errorMessage = exports.getProductById = exports.products = exports.taskQueue = void 0;
exports.taskQueue = 'durable-delivery';
exports.products = [
    {
        id: 1,
        name: 'Swordfish',
        description: 'Cabbage, carrot, well plated',
        image: {
            src: '/swordfish.jpg',
            alt: 'Plated swordfish',
            width: 1920,
            height: 1920
        },
        cents: 3500
    },
    {
        id: 2,
        name: 'Burrata',
        description: 'Fig, peach, cherry tomato',
        image: {
            src: '/burrata.jpg',
            alt: 'Burrata fruit bowl',
            width: 1920,
            height: 1920
        },
        cents: 2000
    },
    {
        id: 3,
        name: 'Potato',
        description: 'Hasselback potatoes, bell pepper salad',
        image: {
            src: '/potatoes.jpg',
            alt: 'Potato dish',
            width: 1920,
            height: 2400
        },
        cents: 1500
    },
    {
        id: 4,
        name: 'Poke',
        description: 'Salmon, cucumber, seaweed, edamame',
        image: {
            src: '/poke.jpg',
            alt: 'Poke bowl',
            width: 1920,
            height: 1920
        },
        cents: 2000
    },
];
function getProductById(id) {
    return exports.products.find(function (product) { return product.id === id; });
}
exports.getProductById = getProductById;
function errorMessage(error) {
    if (typeof error === 'string') {
        return error;
    }
    if (error instanceof Error) {
        return error.message;
    }
    return undefined;
}
exports.errorMessage = errorMessage;
exports.statusColors = {
    'Charging card': 'gray',
    Paid: 'indigo',
    'Picked up': 'yellow',
    Delivered: 'green',
    Failed: 'red'
};
