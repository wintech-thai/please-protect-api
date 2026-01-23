#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/Subnet/org/#{orgId}/action/AddSubnet"
param = {
  Cidr: "192.168.2.0/24",
  Name: "Home net internal #2",
  Tags: "internal",
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
