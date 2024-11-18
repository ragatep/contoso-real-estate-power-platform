import React from 'react';
import './css/slider.css';
import Listings from './components/Listings';
import ListingDetails from './components/ListingDetails';
import CheckoutComplete from './components/CheckoutComplete';

const App = () => {
    const href = window.location.href;

    switch (true) {
        case href.includes('Listing-Details'):
            return React.createElement(ListingDetails);

        case href.includes('Checkout-Complete'):
            return React.createElement(CheckoutComplete);

        default:
            return React.createElement(Listings);
    }
};

export default App;
