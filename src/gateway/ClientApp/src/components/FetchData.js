import React, { Component } from 'react';

export class FetchData extends Component {
  static displayName = FetchData.name;

  constructor(props) {
    super(props);
    this.state = { forecasts: [], loading: true };
  }

  componentDidMount() {
    this.populateWeatherData();
  }

  static renderForecastsTable(forecasts) {
      return (
          <div>
              {forecasts}
          </div>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : FetchData.renderForecastsTable(this.state.forecasts);

    return (
      <div>
        <h1 id="tabelLabel" >Admin Page</h1>
        <p>This components shows the results from calling the admin webapi</p>
        {contents}
      </div>
    );
  }

    async populateWeatherData() {
        console.log('calling http://localhost:8080/admin');
        const response = await fetch('http://localhost:8080/admin');
        const data = await response.json();
        this.setState({ forecasts: data, loading: false });
  }
}
