/* eslint-disable no-undef */
module.exports = {
    preset: 'ts-jest',
    testEnvironment: 'jsdom',
    setupFilesAfterEnv: ['<rootDir>/jest.setup.js'],
    moduleNameMapper: {
        '\\.(css|less|scss|sass)$': '<rootDir>/PortalReactUI/__mocks__/styleMock.js',
        '^@/(.*)$': '<rootDir>/PortalReactUI/$1'
    },
    transform: {
        '^.+\\.(ts|tsx)$': ['ts-jest', {
            tsconfig: 'tsconfig.json'
        }]
    },
    testMatch: ['**/__tests__/**/*.test.(ts|tsx)']
};
