import React, { Component } from 'react';

export class FetchData extends Component {
    static displayName = FetchData.name;

    constructor(props) {
        super(props);
        this.state = { adminData: '', loading: true };
    }

    componentDidMount() {
        this.getAdminData();
    }

    static render(data) {
        console.log(`found ${data}`);

        return (
            <div>
                Results: {data}
            </div>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : FetchData.render(this.state.adminData);

        return (
            <div>
                <h1 id="tabelLabel" >Admin Page</h1>
                <p>This components shows the results from calling the admin webapi</p>
                {contents}
            </div>
        );
    }

    async getAdminData() {
        console.log('calling http://localhost:8080/admin');
        try {
            let data = '';
            const response = await fetch('http://localhost:8080/admin');
            data = await response.json();

            this.setState({ adminData: data, loading: false });
        } catch (e) {
            console.error(e);
        }
    }
}
