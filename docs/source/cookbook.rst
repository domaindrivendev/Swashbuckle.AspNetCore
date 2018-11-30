Cookbook
========

.. toctree::
   :maxdepth: 1

   cookbook/including-xml-comments.rst
   cookbook/listing-response-types.rst
   cookbook/forms-and-file-uploads.rst
   cookbook/xml-media-types.rst
   cookbook/describing-api-security.rst

..
    * [Provide Global API Metadata](#provide-global-api-metadata)
    * [Include Descriptions from XML Comments](#include-descriptions-from-xml-comments) => XmlComments
    * [Handle Forms and File Uploads](#handle-forms-and-file-uploads) => FormMediaTypes
    * [List Operations Responses](#list-operation-responses) => ListingResponsTypes
    * [Add Security Definitions and Requirements](#add-security-definitions-and-requirements) => SecuritySchemes


    * [Omit Obsolete Operations and/or Schema Properties](#omit-obsolete-operations-andor-schema-properties) =>
    * [Omit Arbitrary Operations](#omit-arbitrary-operations)

    * [Flag Required Parameters and Schema Properties](#flag-required-parameters-and-schema-properties) => MvcAnnotations
    * [Assign Explicit OperationIds](#assign-explicit-operationids) NSwagClientExample
    * [Generate Multiple Swagger Documents](#generate-multiple-swagger-documents) => MultipleApiVersions
    * [Inheritance and Polymorphism](#inheritance-and-polymorphism) => NSwagClientExample
..
    * [Change the Path for Swagger JSON Endpoints](#change-the-path-for-swagger-json-endpoints)
    * [Modify Swagger with Request Context](#modify-swagger-with-request-context)
    * [Serialize Swagger JSON in the 2.0 format](#serialize-swagger-in-the-20-format)
    * [Working with Reverse Proxies and Load Balancers](#working-with-reverse-proxies-and-load-balancers)

    * [Customize Operation Tags (e.g. for UI Grouping)](#customize-operation-tags-eg-for-ui-grouping)
    * [Change Operation Sort Order (e.g. for UI Sorting)](#change-operation-sort-order-eg-for-ui-sorting)
    * [Customize Schema Id's](#customize-schema-ids)
    * [Override Schema for Specific Types](#override-schema-for-specific-types)
    * [Extend Generator with Operation, Schema & Document Filters](#extend-generator-with-operation-schema--document-filters)

    * [Change Releative Path to the UI](#change-relative-path-to-the-ui)
    * [Change Document Title](#change-document-title)
    * [List Multiple Swagger Documents](#list-multiple-swagger-documents)
    * [Apply swagger-ui Parameters](#apply-swagger-ui-parameters)
    * [Inject Custom CSS](#inject-custom-css)
    * [Customize index.html](#customize-indexhtml)
    * [Enable OAuth2.0 Flows](#enable-oauth20-flows)
    * [Use client-side request and response interceptors](#use-client-side-request-and-response-interceptors)

    * [Install and Enable Annotations](#install-and-enable-annotations)
    * [Enrich Operation Metadata](#enrich-operation-metadata)
    * [Enrich Response Metadata](#enrich-response-metadata)
    * [Enrich Parameter Metadata](#enrich-parameter-metadata)
    * [Enrich RequestBody Metadata](#enrich-requestbody-metadata)
    * [Enrich Schema Metadata](#enrich-schema-metadata)
    * [Apply Schema Filters to Specific Types](#apply-schema-filters-to-specific-types)
    * [Add Tag Metadata](#add-tag-metadata)

    * [Retrieve Swagger Directly from a Startup Assembly](#retrieve-swagger-directly-from-a-startup-assembly)
    * [Use the CLI Tool with a Custom Host Configuration](#use-the-cli-tool-with-a-custom-host-configuration)

    * [Change Releative Path to the UI](#redoc-change-relative-path-to-the-ui)
    * [Change Document Title](#redoc-change-document-title)
    * [Apply ReDoc Parameters](#apply-redoc-parameters)
    * [Inject Custom CSS](#redoc-inject-custom-css)
    * [Customize index.html](#redoc-customize-indexhtml)

    * Custom serializers