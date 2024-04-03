## 1. 利用できる3D都市モデル

------

本MODでは、3D都市モデル標準製品仕様書（2.3版、第3.4版）に準拠した3D都市モデルの地形、土地利用、道路、建築物、都市計画決定情報の形状（LOD1）と属性を活用します。

| 地物       | 地物型            | 属性区分 | 属性名                                 | 内容                 |
| ---------- | --------------| --------- | --------------------------- | --------- | 
 |共通	  |core:CityModel  |空間属性  |gml:lowerCorne、gml:upperCorner  |データ範囲 |
 |地形  |dem:TINRelief		 |空間属性  |dem:tin/gml:posList							 |形状 |
 |土地利用  |luse:LandUse  |空間属性  |luse:class								 |土地利用用途 |
 |道路  |tran:Road			  |空間属性  |tran:lod1MultiSurface、gml:posList  |形状 |
  |			 |								|主題属性		  |tran:function										 |機能 |
  |			 |								|			  |uro:width											|幅員 |
  |			|							|					 |uro:widthType								|幅員区分 |
 |建築物  |bldg:Building  |空間属性	 |bldg:lod1Solid、gml:posList				 |形状 |
  |			 |							|主題属性	  |bldg:storeysAboveGround				|地上階数 |
  |			|						 |					 |bldg:measuredHeight						  |計測高さ |
  |			|						 |					 |bldg:usage													 |用途 |
  |			|						 |					 |uro:buildingID										  |建物ID |
  |			|						 |					 |uro:orgUsage、uro:orgUsage2 			|詳細用途 |
  |			|						 |					 |gml:name											  |名称 |
 |都市計画決定情報  |urf:UrbanPlanningArea  |空間属性  |urf:lod1MultiSurface、gml:posList  |形状 |
  |			|						 |	主題属性			  |urf:function										 |用途地域の区分 |
