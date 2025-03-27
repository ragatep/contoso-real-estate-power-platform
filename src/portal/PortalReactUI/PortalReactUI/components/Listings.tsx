// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
import React from 'react';
import { Row, Col, Container, InputGroup, FormControl, Card, FormCheck } from 'react-bootstrap';
import ListingCard from './ListingCard';
import { BiSearch } from 'react-icons/bi';
import { XrmContextDataverseClient } from 'dataverse-ify';
import { contoso_listing } from '../dataverse-gen/entities/contoso_listing';
import { ListingCardPlaceholder } from './ListingCardPlaceholder';
import InputRange from 'react-input-range';
import 'react-input-range/lib/css/index.css';
import { FeatureLookup, ListingFeatures } from '../model/features';
import { getListings } from '../api/listingsApi';

interface PriceRange {
    min: number;
    max: number;
}

export interface ListingsProps {
    serviceClient: XrmContextDataverseClient;
}

const Listings: React.FC<ListingsProps> = ({ serviceClient }) => {
    const [isLoading, setIsLoading] = React.useState(true);
    const [searchInput, setSearchInput] = React.useState('');
    const [priceSearch, setPriceSearch] = React.useState<PriceRange>({ min: 400, max: 5000 });
    const [listingData, setListingData] = React.useState<contoso_listing[]>([]);

    const getResponse = React.useCallback(async () => {
        // Get the listings
        const listings = await getListings(serviceClient);
        setListingData(listings);
        setIsLoading(false);
    }, [serviceClient]);

    React.useEffect(() => {
        getResponse();
    }, [getResponse]);

    const [features, setFeatures] = React.useState<ListingFeatures>({
        Garden: false,
        Parking: false,
        Pool: false,
        Gym: false,
        Security: false,
    });
    const handleFeatureChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const { name, checked } = event.target;
        setFeatures((prevFeatures) => ({
            ...prevFeatures,
            [name]: checked,
        }));
    };

    const filteredListings = React.useMemo(() => {
        return listingData.filter((item) => {
            const { contoso_name, contoso_description, contoso_pricepermonth, contoso_features } = item;
            const selectedFeatures = Object.entries(features)
                .filter(([, value]) => value)
                .map(([key]) => FeatureLookup[key as keyof typeof FeatureLookup]);

            return (
                (contoso_name?.toLowerCase().includes(searchInput.toLowerCase()) ||
                    contoso_description?.toLowerCase().includes(searchInput.toLowerCase())) &&
                (contoso_pricepermonth ?? 0) >= priceSearch.min &&
                (contoso_pricepermonth ?? 0) <= priceSearch.max &&
                selectedFeatures.every((feature) => contoso_features?.includes(feature))
            );
        });
    }, [searchInput, listingData, priceSearch, features]);

    return (
        <>
            <Container fluid>
                <Row className="justify-content-center">
                    <Col xs={10} md={4} lg={4} xl={4} className="">
                        <InputGroup className="flex-nowrap">
                            <InputGroup.Text className={'bg-light text-light-primary'}>
                                <BiSearch size={20} aria-hidden={true} />
                            </InputGroup.Text>
                            <FormControl
                                placeholder="Search"
                                value={searchInput}
                                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setSearchInput(e.target.value)}
                                className={'bg-light text-black search-listings-search-input'}
                            />
                        </InputGroup>
                    </Col>
                    <Col xs={10} md={4} lg={4} xl={4} className="py-3">
                        <Row className="flex-nowrap">
                            <Col className="col-auto text-nowrap">
                                <label>Price/month</label>
                            </Col>
                            <Col xs={6}>
                                <div className="input-range-wrapper">
                                    <InputRange
                                        maxValue={5000}
                                        minValue={0}
                                        value={priceSearch}
                                        onChange={(value: number | PriceRange) => setPriceSearch(value as PriceRange)}
                                    />
                                </div>
                            </Col>
                        </Row>
                    </Col>
                </Row>

                <Row className="justify-content-center">
                    {Object.keys(features).map((feature, index) => (
                        <Col key={index} className="col-auto text-nowrap">
                            <FormCheck
                                type="checkbox"
                                id={`checkbox-${index}`}
                                label={feature}
                                name={feature}
                                checked={features[feature as keyof ListingFeatures]}
                                onChange={handleFeatureChange}
                            />
                        </Col>
                    ))}
                </Row>

                <div className="bg-light-2" style={{ height: '100vh', width: 'auto', overflowY: 'auto' }}>
                    <Container fluid className="py-4">
                        <Row className="d-flex gap-3 m-0">
                            {isLoading ? (
                                Array.from({ length: 10 }).map((_, index) => (
                                    <Card
                                        key={index}
                                        style={{ width: '18rem', height: 'auto' }}
                                        className={
                                            'bg-light text-black text-center p-0 overflow-hidden shadow mx-auto mb-4'
                                        }
                                    >
                                        <ListingCardPlaceholder />
                                    </Card>
                                ))
                            ) : filteredListings.length === 0 ? (
                                <Container fluid className="d-flex justify-content-center align-items-center">
                                    No matching listings found
                                </Container>
                            ) : (
                                filteredListings.map((item) => (
                                    <ListingCard listing={item} key={item.contoso_listingid} />
                                ))
                            )}
                        </Row>
                    </Container>
                </div>
            </Container>
        </>
    );
};

export default Listings;
